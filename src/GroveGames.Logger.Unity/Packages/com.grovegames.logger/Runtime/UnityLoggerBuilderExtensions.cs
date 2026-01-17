namespace GroveGames.Logger.Unity
{
    public static class UnityLoggerBuilderExtensions
    {
        public static void AddUnityFileLogProcessor(this ILoggerBuilder builder)
        {
            var settings = UnityLoggerSettings.Load();
            AddUnityFileLogProcessor(builder, settings);
        }

        public static void AddUnityFileLogProcessor(this ILoggerBuilder builder, UnityLoggerSettings settings)
        {
            var unityFileFactory = new UnityLogFileFactory(settings.FileFolderName, settings.MaxFileCount, settings.FileBufferSize);
            var streamWriter = new StreamWriter(unityFileFactory.CreateFile(), settings.FileBufferSize, settings.FileChannelCapacity);
            var fileLogFormatter = new FileLogFormatter();
            builder.AddLogProcessor(new FileLogProcessor(streamWriter, fileLogFormatter));
        }

        public static void AddUnityConsoleLogProcessor(this ILoggerBuilder builder)
        {
            var unityConsoleLogFormatter = new UnityConsoleLogFormatter();
            builder.AddLogProcessor(new UnityConsoleLogProcessor(unityConsoleLogFormatter));
        }
    }
}
