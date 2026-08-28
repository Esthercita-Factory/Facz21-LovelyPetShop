using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.Business.Services;

public class LoggerService : ILoggerService
{
    private readonly string _logFilePath;
    private static readonly SemaphoreSlim _fileLock = new(1, 1);

    public LoggerService(string? logFilePath = null)
    {
        _logFilePath = logFilePath ?? Path.Combine(AppContext.BaseDirectory, "clinic_events.log");
    }

    public async Task LogInfoAsync(string message)
    {
        await WriteLogEntryAsync("INFO", message);
    }

    public async Task LogWarningAsync(string message)
    {
        await WriteLogEntryAsync("WARN", message);
    }

    public async Task LogErrorAsync(string message, Exception? ex = null)
    {
        string fullMessage = ex != null ? $"{message} | Excepción: {ex.GetType().Name} - {ex.Message}" : message;
        await WriteLogEntryAsync("ERROR", fullMessage);
    }

    public async Task<IEnumerable<string>> GetRecentLogsAsync(int count = 20)
    {
        await _fileLock.WaitAsync();
        try
        {
            if (!File.Exists(_logFilePath))
                return Enumerable.Empty<string>();

            var lines = await File.ReadAllLinesAsync(_logFilePath);
            return lines.TakeLast(count).Reverse();
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task WriteLogEntryAsync(string level, string message)
    {
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

        await _fileLock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(_logFilePath, entry + Environment.NewLine);
        }
        catch
        {
            // Evitar que un fallo de logging interrumpa la aplicación
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
