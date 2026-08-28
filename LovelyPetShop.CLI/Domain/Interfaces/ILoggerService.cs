namespace LovelyPetShop.CLI.Domain.Interfaces;

/// <summary>
/// Interfaz para el servicio de registro estructurado de eventos y errores (Logging).
/// </summary>
public interface ILoggerService
{
    Task LogInfoAsync(string message);
    Task LogWarningAsync(string message);
    Task LogErrorAsync(string message, Exception? ex = null);
    Task<IEnumerable<string>> GetRecentLogsAsync(int count = 20);
}
