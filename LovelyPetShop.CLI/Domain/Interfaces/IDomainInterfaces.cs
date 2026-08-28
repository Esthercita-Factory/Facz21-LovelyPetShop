using LovelyPetShop.CLI.Domain.Entities;

namespace LovelyPetShop.CLI.Domain.Interfaces;

/// <summary>
/// Interfaz para entidades que pueden ser registradas en el sistema de la clínica.
/// </summary>
public interface IRegistrable
{
    string ObtenerResumenRegistro();
}

/// <summary>
/// Interfaz para servicios o atenciones clínicas veterinarias.
/// </summary>
public interface IAtendible
{
    string Atender(Pet pet);
}

/// <summary>
/// Interfaz para entidades capaces de recibir o emitir notificaciones.
/// </summary>
public interface INotificable
{
    Task<string> EnviarNotificacionAsync(string mensaje);
}
