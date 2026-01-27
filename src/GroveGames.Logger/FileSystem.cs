namespace GroveGames.Logger;

public class FileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public Stream CreateFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        => new FileStream(path, mode, access, share, bufferSize, useAsync);

    public void DeleteFile(string path) => File.Delete(path);

    public IEnumerable<FileInfo> GetFiles(string path, string searchPattern)
    {
        var directoryInfo = new DirectoryInfo(path);
        return directoryInfo.GetFiles(searchPattern)
            .Select(f => new FileInfo(f.FullName, f.CreationTime));
    }
}
