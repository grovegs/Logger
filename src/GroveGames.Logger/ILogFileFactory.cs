using System;

namespace GroveGames.Logger;

public interface ILogFileFactory
{
    StreamWriter CreateFile();
}
