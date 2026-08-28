using LovelyPetShop.CLI.Domain.Entities;
using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.Business.Services;

public class LinqReportService : ILinqReportService
{
    private readonly IPetRepository _petRepository;
    private readonly IOwnerRepository _ownerRepository;

    public LinqReportService(IPetRepository petRepository, IOwnerRepository ownerRepository)
    {
        _petRepository = petRepository;
        _ownerRepository = ownerRepository;
    }

    /// <summary>
    /// Agrupa mascotas por especie, calculando la cantidad y los promedios de edad y peso usando GroupBy, Count y Average.
    /// </summary>
    public async Task<IEnumerable<EspecieConteoDto>> AgruparMascotasPorEspecieAsync()
    {
        var pets = (await _petRepository.GetAllAsync()).ToList();

        // LINQ: GroupBy + Select para proyectar métricas agregadas
        var agrupado = pets
            .GroupBy(p => string.IsNullOrWhiteSpace(p.Species) ? "Sin especificar" : p.Species.Trim())
            .Select(g => new EspecieConteoDto(
                Especie: g.Key,
                Cantidad: g.Count(),
                PromedioEdad: g.Any() ? Math.Round(g.Average(p => p.Age), 1) : 0,
                PromedioPeso: g.Any() ? Math.Round(g.Average(p => p.Weight), 1) : 0
            ))
            .OrderByDescending(dto => dto.Cantidad)
            .ToList();

        return agrupado;
    }

    /// <summary>
    /// Encuentra la mascota más joven y la de mayor edad usando OrderBy y FirstOrDefault.
    /// </summary>
    public async Task<(Pet? MasJoven, Pet? MasViejo)> ObtenerExtremosEdadMascotasAsync()
    {
        var pets = (await _petRepository.GetAllAsync()).ToList();
        if (!pets.Any()) return (null, null);

        var masJoven = pets.OrderBy(p => p.Age).FirstOrDefault();
        var masViejo = pets.OrderByDescending(p => p.Age).FirstOrDefault();

        return (masJoven, masViejo);
    }

    /// <summary>
    /// Consulta encadenada que filtra mascotas por especie, las ordena por edad ascendente,
    /// cruza con los datos del propietario y proyecta solo la información de contacto requerida.
    /// </summary>
    public async Task<IEnumerable<MascotaContactoDto>> FiltrarMascotasPorEspecieYOrdenarAsync(string especie)
    {
        var pets = (await _petRepository.GetAllAsync()).ToList();
        var owners = (await _ownerRepository.GetAllAsync()).ToList();

        var ownerDict = owners.ToDictionary(o => o.DocumentNumber, o => o, StringComparer.OrdinalIgnoreCase);

        // LINQ: Where -> OrderBy -> Select proyectando DTO
        var resultado = pets
            .Where(p => string.Equals(p.Species, especie?.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Age)
            .ThenBy(p => p.Name)
            .Select(p =>
            {
                ownerDict.TryGetValue(p.OwnerDocumentNumber, out var owner);
                return new MascotaContactoDto(
                    NombreMascota: p.Name,
                    Especie: p.Species,
                    Raza: p.Breed,
                    Edad: p.Age,
                    NombreDueno: owner?.Name ?? "Desconocido",
                    TelefonoDueno: owner?.Phone ?? "Sin teléfono",
                    DocumentoDueno: p.OwnerDocumentNumber
                );
            })
            .ToList();

        return resultado;
    }

    /// <summary>
    /// Verifica si existe al menos una mascota que cumpla una condición específica usando Any.
    /// </summary>
    public async Task<bool> ExisteMascotaConCondicionAsync(Func<Pet, bool> predicado)
    {
        var pets = await _petRepository.GetAllAsync();
        return pets.Any(predicado);
    }

    /// <summary>
    /// Verifica si todas las mascotas cumplen una condición específica usando All.
    /// </summary>
    public async Task<bool> TodasLasMascotasCumplenCondicionAsync(Func<Pet, bool> predicado)
    {
        var pets = await _petRepository.GetAllAsync();
        return pets.Any() && pets.All(predicado);
    }

    /// <summary>
    /// Obtiene todos los nombres de las mascotas transformados a MAYÚSCULAS y ordenados alfabéticamente.
    /// </summary>
    public async Task<IEnumerable<string>> ObtenerNombresMascotasEnMayusculasOrdenadosAsync()
    {
        var pets = await _petRepository.GetAllAsync();

        return pets
            .Select(p => p.Name.ToUpperInvariant())
            .OrderBy(n => n)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Genera un diccionario Dictionary<string, Pet> para acceso y búsqueda rápida en tiempo O(1) por UUID.
    /// </summary>
    public async Task<Dictionary<string, Pet>> ObtenerDiccionarioMascotasPorUuidAsync()
    {
        var pets = await _petRepository.GetAllAsync();
        return pets
            .Where(p => !string.IsNullOrWhiteSpace(p.Uuid))
            .ToDictionary(p => p.Uuid, p => p, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Genera un diccionario Dictionary<string, Owner> para acceso y búsqueda rápida en tiempo O(1) por documento.
    /// </summary>
    public async Task<Dictionary<string, Owner>> ObtenerDiccionarioPropietariosPorDocumentoAsync()
    {
        var owners = await _ownerRepository.GetAllAsync();
        return owners
            .Where(o => !string.IsNullOrWhiteSpace(o.DocumentNumber))
            .GroupBy(o => o.DocumentNumber, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Demuestra la sintaxis de consulta (Query Syntax) de LINQ para filtrar y ordenar.
    /// </summary>
    public async Task<IEnumerable<Pet>> ConsultarConSintaxisQueryAsync(string especieFiltro, int edadMinima)
    {
        var pets = (await _petRepository.GetAllAsync()).ToList();

        // Sintaxis de consulta LINQ (Query Syntax)
        var query = from p in pets
                    where p.Species.Equals(especieFiltro, StringComparison.OrdinalIgnoreCase)
                          && p.Age >= edadMinima
                    orderby p.Name ascending
                    select p;

        return query.ToList();
    }
}
