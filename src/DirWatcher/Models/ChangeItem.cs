namespace DirWatcher.Models;

public sealed class ChangeItem
{
    public required string Path { get; init; }

    public bool IsDirectory { get; init; }

    public int? Version { get; init; }

    public int? PreviousVersion { get; init; }
}
