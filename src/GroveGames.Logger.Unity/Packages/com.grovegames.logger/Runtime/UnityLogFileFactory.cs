using System.IO;
using UnityEngine;

namespace GroveGames.Logger.Unity;

public sealed class UnityLogFileFactory : ILogFileFactory
{
    private readonly LogFileFactory _logFileFactory;

    public UnityLogFileFactory(string fileFolderName, int maxFileCount, int bufferSize)
    {
        _logFileFactory = new LogFileFactory(Application.persistentDataPath, fileFolderName, maxFileCount, bufferSize);
    }

    public Stream CreateFile()
    {
        return _logFileFactory.CreateFile();
    }
}
