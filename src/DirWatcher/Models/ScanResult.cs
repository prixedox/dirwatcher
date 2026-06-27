namespace DirWatcher.Models;

public sealed class ScanResult
{
    public required string AnalyzedPath { get; init; }

    public bool IsInitialScan { get; init; }

    public DateTime ScannedAtUtc { get; init; }

    public List<ChangeItem> Added { get; init; } = new();

    public List<ChangeItem> Changed { get; init; } = new();

    public List<ChangeItem> Deleted { get; init; } = new();

    public int TrackedFileCount { get; init; }

    public int TrackedDirectoryCount { get; init; }

    public bool HasChanges => Added.Count > 0 || Changed.Count > 0 || Deleted.Count > 0;
}
