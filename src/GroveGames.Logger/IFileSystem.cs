namespace GroveGames.Logger;

public interface IFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    Stream CreateFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync);
    IEnumerable<FileInfo> GetFiles(string path, string searchPattern);
    void DeleteFile(string path);
}
