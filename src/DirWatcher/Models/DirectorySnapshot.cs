namespace DirWatcher.Models;

public sealed class DirectorySnapshot
{
    public required string AnalyzedPath { get; init; }

    public DateTime CapturedAtUtc { get; init; }

    public List<FileEntry> Entries { get; init; } = new();
}
