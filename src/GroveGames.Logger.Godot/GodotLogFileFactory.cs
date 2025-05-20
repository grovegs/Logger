using Godot;

namespace GroveGames.Logger;

public sealed class GodotLogFileFactory : ILogFileFactory
{
    private readonly LogFileFactory _logFileFactory;

    public GodotLogFileFactory(string fileFolderName, int maxFileCount, int bufferSize)
    {
        _logFileFactory = new LogFileFactory(OS.GetUserDataDir(), fileFolderName, maxFileCount, bufferSize);
    }

    public FileStream CreateFile()
    {
        return _logFileFactory.CreateFile();
    }
}
