using DirWatcher.Services;

namespace DirWatcher.Tests;

public sealed class ChangeDetectionServiceTests : IDisposable
{
    private readonly string _root;
    private readonly string _snapshotDir;
    private readonly ChangeDetectionService _service;

    public ChangeDetectionServiceTests()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "dirwatcher-tests", Guid.NewGuid().ToString("N"));
        _root = Path.Combine(baseDir, "content");
        _snapshotDir = Path.Combine(baseDir, "snapshots");
        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(_snapshotDir);
        _service = new ChangeDetectionService(new JsonSnapshotStore(_snapshotDir));
    }

    public void Dispose()
    {
        try { Directory.Delete(Path.GetDirectoryName(_root)!, recursive: true); } catch { }
    }

    private void Write(string name, string content) =>
        File.WriteAllText(Path.Combine(_root, name), content);

    [Fact]
    public void First_scan_is_baseline_with_no_changes()
    {
        Write("a.txt", "hello");

        var result = _service.Analyze(_root);

        Assert.True(result.IsInitialScan);
        Assert.False(result.HasChanges);
        Assert.Equal(1, result.TrackedFileCount);
    }

    [Fact]
    public void New_file_is_added_with_version_1()
    {
        Write("a.txt", "hello");
        _service.Analyze(_root);

        Write("b.txt", "world");
        var result = _service.Analyze(_root);

        var added = Assert.Single(result.Added);
        Assert.Equal("b.txt", added.Path);
        Assert.Equal(1, added.Version);
    }

    [Fact]
    public void Changed_content_bumps_the_version()
    {
        Write("a.txt", "hello");
        _service.Analyze(_root);

        Write("a.txt", "hello world");
        var result = _service.Analyze(_root);

        var changed = Assert.Single(result.Changed);
        Assert.Equal("a.txt", changed.Path);
        Assert.Equal(1, changed.PreviousVersion);
        Assert.Equal(2, changed.Version);
    }

    [Fact]
    public void Deleted_file_is_reported()
    {
        Write("a.txt", "hello");
        Write("b.txt", "world");
        _service.Analyze(_root);

        File.Delete(Path.Combine(_root, "b.txt"));
        var result = _service.Analyze(_root);

        var deleted = Assert.Single(result.Deleted);
        Assert.Equal("b.txt", deleted.Path);
    }

    [Fact]
    public void Unchanged_file_is_not_reported()
    {
        Write("a.txt", "hello");
        _service.Analyze(_root);

        var result = _service.Analyze(_root);

        Assert.False(result.HasChanges);
    }

    [Fact]
    public void New_subdirectory_is_reported()
    {
        Write("a.txt", "hello");
        _service.Analyze(_root);

        Directory.CreateDirectory(Path.Combine(_root, "sub"));
        var result = _service.Analyze(_root);

        var added = Assert.Single(result.Added);
        Assert.Equal("sub", added.Path);
        Assert.True(added.IsDirectory);
    }

    [Fact]
    public void Missing_directory_throws()
    {
        Assert.Throws<DirectoryNotFoundException>(
            () => _service.Analyze(Path.Combine(_root, "nope")));
    }
}
