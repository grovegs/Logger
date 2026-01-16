namespace GroveGames.Logger;

public static class GodotSettings
{
    public static readonly GodotSetting<LogLevel> MinLogLevel = new("grove_games/logger/min_log_level", LogLevel.Information);
    public static readonly GodotSetting<int> MaxFileCount = new("grove_games/logger/max_file_count", 10);
    public static readonly GodotSetting<string> FileFolderName = new("grove_games/logger/file_folder_name", "logs");
    public static readonly GodotSetting<int> FileBufferSize = new("grove_games/logger/file_buffer_size", 8 * 1024);
    public static readonly GodotSetting<int> FileChannelCapacity = new("grove_games/logger/file_channel_capacity", 1000);

    public static void CreateIfNotExist()
    {
        MinLogLevel.CreateIfNotExist();
        MaxFileCount.CreateIfNotExist();
        FileFolderName.CreateIfNotExist();
        FileBufferSize.CreateIfNotExist();
        FileChannelCapacity.CreateIfNotExist();
    }
}
