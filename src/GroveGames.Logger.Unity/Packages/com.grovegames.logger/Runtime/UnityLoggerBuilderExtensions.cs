namespace GroveGames.Logger.Unity
{
    public static class UnityLoggerBuilderExtensions
    {
        public static void AddUnityFileLogProcessor(this ILoggerBuilder builder)
        {
            var settings = UnityLoggerSettings.GetOrCreate();
            AddUnityFileLogProcessor(builder, settings);
        }

        public static void AddUnityFileLogProcessor(this ILoggerBuilder builder, UnityLoggerSettings settings)
        {
            UnityLogFileFactory unityFileFactory = new(settings.FileFolderName, settings.MaxFileCount, settings.FileBufferSize);
            StreamWriter streamWriter = new(unityFileFactory.CreateFile(), settings.FileBufferSize, settings.FileChannelCapacity);
            FileLogFormatter fileLogFormatter = new();
            builder.AddLogProcessor(new FileLogProcessor(streamWriter, fileLogFormatter));
        }

        public static void AddUnityConsoleLogProcessor(this ILoggerBuilder builder)
        {
            UnityConsoleLogFormatter unityConsoleLogFormatter = new();
            builder.AddLogProcessor(new UnityConsoleLogProcessor(unityConsoleLogFormatter));
        }
    }
}
