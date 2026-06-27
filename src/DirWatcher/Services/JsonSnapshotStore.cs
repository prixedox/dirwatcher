using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DirWatcher.Models;

namespace DirWatcher.Services;

public sealed class JsonSnapshotStore : ISnapshotStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _baseDirectory;

    public JsonSnapshotStore(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        Directory.CreateDirectory(_baseDirectory);
    }

    public DirectorySnapshot? Load(string analyzedPath)
    {
        var file = FilePathFor(analyzedPath);
        if (!File.Exists(file))
            return null;

        using var stream = File.OpenRead(file);
        return JsonSerializer.Deserialize<DirectorySnapshot>(stream, Options);
    }

    public void Save(DirectorySnapshot snapshot)
    {
        var file = FilePathFor(snapshot.AnalyzedPath);
        File.WriteAllText(file, JsonSerializer.Serialize(snapshot, Options));
    }

    public void Delete(string analyzedPath)
    {
        var file = FilePathFor(analyzedPath);
        if (File.Exists(file))
            File.Delete(file);
    }

    private string FilePathFor(string analyzedPath)
    {
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(analyzedPath)));
        return Path.Combine(_baseDirectory, key + ".json");
    }
}
