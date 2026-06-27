using System.Security.Cryptography;
using DirWatcher.Models;

namespace DirWatcher.Services;

public sealed class ChangeDetectionService
{
    private readonly ISnapshotStore _store;

    public ChangeDetectionService(ISnapshotStore store)
    {
        _store = store;
    }

    public ScanResult Analyze(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            throw new ArgumentException("Path must not be empty.", nameof(directoryPath));

        var root = Path.GetFullPath(directoryPath);
        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException($"Directory not found: {root}");

        var now = DateTime.UtcNow;
        var current = ScanDirectory(root);
        var previous = _store.Load(root);

        var prevByPath = (previous?.Entries ?? new List<FileEntry>())
            .ToDictionary(e => e.RelativePath);

        var added = new List<ChangeItem>();
        var changed = new List<ChangeItem>();
        var deleted = new List<ChangeItem>();
        var newEntries = new List<FileEntry>(current.Count);
        var seen = new HashSet<string>();
        var firstRun = previous is null;

        foreach (var item in current)
        {
            seen.Add(item.RelativePath);
            prevByPath.TryGetValue(item.RelativePath, out var prior);

            if (item.IsDirectory)
            {
                if (prior is null && !firstRun)
                    added.Add(new ChangeItem { Path = item.RelativePath, IsDirectory = true });
                newEntries.Add(WithVersion(item, 0));
                continue;
            }

            if (prior is null || prior.IsDirectory)
            {
                if (!firstRun)
                    added.Add(new ChangeItem { Path = item.RelativePath, IsDirectory = false, Version = 1 });
                newEntries.Add(WithVersion(item, 1));
            }
            else if (prior.Hash != item.Hash)
            {
                var version = prior.Version + 1;
                changed.Add(new ChangeItem
                {
                    Path = item.RelativePath,
                    IsDirectory = false,
                    Version = version,
                    PreviousVersion = prior.Version,
                });
                newEntries.Add(WithVersion(item, version));
            }
            else
            {
                newEntries.Add(WithVersion(item, prior.Version));
            }
        }

        if (!firstRun)
        {
            foreach (var prior in previous!.Entries)
            {
                if (seen.Contains(prior.RelativePath))
                    continue;

                deleted.Add(new ChangeItem
                {
                    Path = prior.RelativePath,
                    IsDirectory = prior.IsDirectory,
                    Version = prior.IsDirectory ? null : prior.Version,
                });
            }
        }

        _store.Save(new DirectorySnapshot
        {
            AnalyzedPath = root,
            CapturedAtUtc = now,
            Entries = newEntries,
        });

        return new ScanResult
        {
            AnalyzedPath = root,
            IsInitialScan = firstRun,
            ScannedAtUtc = now,
            Added = added,
            Changed = changed,
            Deleted = deleted,
            TrackedFileCount = newEntries.Count(e => !e.IsDirectory),
            TrackedDirectoryCount = newEntries.Count(e => e.IsDirectory),
        };
    }

    private static List<FileEntry> ScanDirectory(string root)
    {
        var entries = new List<FileEntry>();

        foreach (var dir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
        {
            entries.Add(new FileEntry
            {
                RelativePath = ToRelative(root, dir),
                IsDirectory = true,
            });
        }

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var info = new FileInfo(file);
            entries.Add(new FileEntry
            {
                RelativePath = ToRelative(root, file),
                IsDirectory = false,
                Hash = ComputeHash(file),
                Size = info.Length,
                LastModifiedUtc = info.LastWriteTimeUtc,
            });
        }

        return entries;
    }

    private static string ToRelative(string root, string fullPath) =>
        Path.GetRelativePath(root, fullPath).Replace(Path.DirectorySeparatorChar, '/');

    private static string ComputeHash(string file)
    {
        using var stream = File.OpenRead(file);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static FileEntry WithVersion(FileEntry scanned, int version) => new()
    {
        RelativePath = scanned.RelativePath,
        IsDirectory = scanned.IsDirectory,
        Hash = scanned.Hash,
        Version = version,
        Size = scanned.Size,
        LastModifiedUtc = scanned.LastModifiedUtc,
    };
}
