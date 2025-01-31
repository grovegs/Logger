using Godot;

namespace GroveGames.Logger;

public class GodotLogFileFactory : ILogFileFactory
{
    private readonly LogFileFactory _logFileFactory;

    public GodotLogFileFactory()
    {
        _logFileFactory = new LogFileFactory(OS.GetUserDataDir());
    }

    public StreamWriter CreateFile()
    {
        return _logFileFactory.CreateFile();
    }
}
