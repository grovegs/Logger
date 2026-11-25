using System;
using System.IO;

namespace GroveGames.Logger.Unity
{
    public static class LoggerBuilderExtensions
    {
        public static void AddFileLogProcessor(this ILoggerBuilder builder)
        {
            var settings = UnityLoggerSettings.GetOrCreate();
            AddFileLogProcessor(builder, settings);
        }

        public static void AddFileLogProcessor(this ILoggerBuilder builder, ILoggerSettings settings)
        {
            if (string.IsNullOrEmpty(settings.FileFolderName) || settings.MaxFileCount <= 0 || settings.FileBufferSize <= 0 || settings.FileChannelCapacity <= 0)
            {
                UnityEngine.Debug.LogError($"File logging is off: the logger settings need a folder name and a positive file count, buffer size and channel capacity, but they are '{settings.FileFolderName}', {settings.MaxFileCount}, {settings.FileBufferSize} and {settings.FileChannelCapacity}.");
                return;
            }

            Stream? stream = null;

            try
            {
                var unityFileFactory = new UnityLogFileFactory(settings.FileFolderName, settings.MaxFileCount, settings.FileBufferSize);
                stream = unityFileFactory.CreateFile();
                var streamWriter = new StreamWriter(stream, settings.FileBufferSize, settings.FileChannelCapacity);
                builder.AddLogProcessor(new FileLogProcessor(streamWriter, new FileLogFormatter()));
                builder.AddLogSource(_ => new LogFileFlusher(streamWriter));
            }
            catch (Exception exception)
            {
                stream?.Dispose();
                UnityEngine.Debug.LogError($"File logging is off: the log file could not be created. {exception}");
            }
        }

        public static void AddConsoleLogProcessor(this ILoggerBuilder builder)
        {
            var unityConsoleLogFormatter = new ConsoleLogFormatter();
            builder.AddLogProcessor(new ConsoleLogProcessor(unityConsoleLogFormatter));
        }

        public static void AddLogHistory(this ILoggerBuilder builder, ILogHistory history)
        {
            builder.AddLogProcessor(new LogHistoryProcessor(history));
        }

        public static void AddLogSource(this ILoggerBuilder builder, string tag = "Unity")
        {
            builder.AddLogSource(processors => new LogSource(processors, tag));
        }
    }
}
