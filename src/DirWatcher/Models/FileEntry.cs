namespace DirWatcher.Models;

public sealed class FileEntry
{
    public required string RelativePath { get; init; }

    public bool IsDirectory { get; init; }

    public string? Hash { get; init; }

    public int Version { get; init; }

    public long Size { get; init; }

    public DateTime LastModifiedUtc { get; init; }
}
