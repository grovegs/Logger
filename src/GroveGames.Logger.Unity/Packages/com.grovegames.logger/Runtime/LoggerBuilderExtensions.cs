namespace GroveGames.Logger.Unity
{
    public static class LoggerBuilderExtensions
    {
        public static void AddFileLogProcessor(this ILoggerBuilder builder)
        {
            var settings = LoggerSettings.GetOrCreate();
            AddFileLogProcessor(builder, settings);
        }

        public static void AddFileLogProcessor(this ILoggerBuilder builder, LoggerSettings settings)
        {
            var unityFileFactory = new UnityLogFileFactory(settings.FileFolderName, settings.MaxFileCount, settings.FileBufferSize);
            var streamWriter = new StreamWriter(unityFileFactory.CreateFile(), settings.FileBufferSize, settings.FileChannelCapacity);
            var fileLogFormatter = new FileLogFormatter();
            builder.AddLogProcessor(new FileLogProcessor(streamWriter, fileLogFormatter));
        }

        public static void AddConsoleLogProcessor(this ILoggerBuilder builder)
        {
            var unityConsoleLogFormatter = new ConsoleLogFormatter();
            builder.AddLogProcessor(new ConsoleLogProcessor(unityConsoleLogFormatter));
        }

        public static void AddLogSource(this ILoggerBuilder builder, string tag = "Unity")
        {
            builder.AddLogSource(processors => new LogSource(processors, tag));
        }
    }
}
