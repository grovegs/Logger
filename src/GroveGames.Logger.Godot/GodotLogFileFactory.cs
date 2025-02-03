using Godot;

namespace GroveGames.Logger;

public sealed class GodotLogFileFactory : ILogFileFactory
{
    private readonly LogFileFactory _logFileFactory;

    public GodotLogFileFactory(string fileFolderName, int maxFileCount)
    {
        _logFileFactory = new LogFileFactory(OS.GetUserDataDir(), fileFolderName, maxFileCount);
    }

    public StreamWriter CreateFile()
    {
        return _logFileFactory.CreateFile();
    }
}
