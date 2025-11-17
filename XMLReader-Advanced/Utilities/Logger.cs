namespace XMLReader_Advanced.Utilities;

public class Logger
{
    private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
    private const string FilePath = "app_log.txt";
    private readonly object _fileLock = new object();

    public static Logger Instance => _instance.Value;

    private Logger()
    {
    }

    public enum LoggingLevel
    {
        Filtering,
        Transformation,
        Saving,
        Error
    }

    public void Log(LoggingLevel level, string message)
    {
        var logEntry = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {level}. {message}";

        lock (_fileLock)
        {
            File.AppendAllText(FilePath, logEntry + Environment.NewLine);
        }
    }
}