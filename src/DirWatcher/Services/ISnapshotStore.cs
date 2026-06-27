using DirWatcher.Models;

namespace DirWatcher.Services;

public interface ISnapshotStore
{
    DirectorySnapshot? Load(string analyzedPath);

    void Save(DirectorySnapshot snapshot);

    void Delete(string analyzedPath);
}
