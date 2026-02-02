namespace GroveGames.Logger;

public readonly struct FileInfo
{
    private readonly string _fullPath;
    private readonly DateTime _creationTime;

    public string FullPath => _fullPath;
    public DateTime CreationTime => _creationTime;

    public FileInfo(string fullPath, DateTime creationTime)
    {
        _fullPath = fullPath;
        _creationTime = creationTime;
    }
}
