namespace GroveGames.Logger;

public interface IFileSystem
{
    public bool DirectoryExists(string path);
    public void CreateDirectory(string path);
    public Stream CreateFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync);
    public IEnumerable<FileInfo> GetFiles(string path, string searchPattern);
    public void DeleteFile(string path);
}
