namespace DirWatcher.Services;

public sealed class DemoFolderService
{
    public string FolderPath { get; }

    public DemoFolderService(string folderPath)
    {
        FolderPath = folderPath;
    }

    public void Reset()
    {
        if (Directory.Exists(FolderPath))
            Directory.Delete(FolderPath, recursive: true);

        Directory.CreateDirectory(Path.Combine(FolderPath, "sub"));
        File.WriteAllText(Path.Combine(FolderPath, "a.txt"), "alpha\n");
        File.WriteAllText(Path.Combine(FolderPath, "b.txt"), "beta\n");
        File.WriteAllText(Path.Combine(FolderPath, "sub", "c.txt"), "nested file\n");
    }

    public void ApplyChanges()
    {
        Directory.CreateDirectory(FolderPath);

        File.WriteAllText(Path.Combine(FolderPath, "a.txt"), $"alpha modified at {DateTime.UtcNow:O}\n");
        File.WriteAllText(Path.Combine(FolderPath, "d.txt"), "delta\n");

        var b = Path.Combine(FolderPath, "b.txt");
        if (File.Exists(b))
            File.Delete(b);

        Directory.CreateDirectory(Path.Combine(FolderPath, "sub2"));
    }
}
