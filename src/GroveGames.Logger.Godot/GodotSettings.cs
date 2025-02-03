namespace GroveGames.Logger;

public static class GodotSettings
{
    public static readonly GodotSetting<LogLevel> MinLogLevel = new("grove_games/logger/min_log_level", LogLevel.Information);
    public static readonly GodotSetting<int> MaxFileCount = new("grove_games/logger/max_file_count", 10);
    public static readonly GodotSetting<string> FileFolderName = new("grove_games/logger/file_folder_name", "logs");
    public static readonly GodotSetting<int> FileWriteInterval = new("grove_games/logger/file_write_interval", 1000);
    public static readonly GodotSetting<int> FileCharacterQueueSize = new("grove_games/logger/file_character_queue_size", 256);
}
