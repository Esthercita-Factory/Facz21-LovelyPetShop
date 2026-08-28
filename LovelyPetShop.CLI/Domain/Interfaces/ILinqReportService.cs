using LovelyPetShop.CLI.Domain.Entities;

namespace LovelyPetShop.CLI.Domain.Interfaces;

public record EspecieConteoDto(string Especie, int Cantidad, double PromedioEdad, double PromedioPeso);
public record MascotaContactoDto(string NombreMascota, string Especie, string Raza, int Edad, string NombreDueno, string TelefonoDueno, string DocumentoDueno);

public interface ILinqReportService
{
    Task<IEnumerable<EspecieConteoDto>> AgruparMascotasPorEspecieAsync();
    Task<(Pet? MasJoven, Pet? MasViejo)> ObtenerExtremosEdadMascotasAsync();
    Task<IEnumerable<MascotaContactoDto>> FiltrarMascotasPorEspecieYOrdenarAsync(string especie);
    Task<bool> ExisteMascotaConCondicionAsync(Func<Pet, bool> predicado);
    Task<bool> TodasLasMascotasCumplenCondicionAsync(Func<Pet, bool> predicado);
    Task<IEnumerable<string>> ObtenerNombresMascotasEnMayusculasOrdenadosAsync();
    Task<Dictionary<string, Pet>> ObtenerDiccionarioMascotasPorUuidAsync();
    Task<Dictionary<string, Owner>> ObtenerDiccionarioPropietariosPorDocumentoAsync();
    Task<IEnumerable<Pet>> ConsultarConSintaxisQueryAsync(string especieFiltro, int edadMinima);
}
