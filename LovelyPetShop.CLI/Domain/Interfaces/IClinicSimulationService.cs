using LovelyPetShop.CLI.Domain.Entities;

namespace LovelyPetShop.CLI.Domain.Interfaces;

public record SimulacionResultado(string Operacion, string Detalle, double DuracionMs);

public interface IClinicSimulationService
{
    Task<IEnumerable<SimulacionResultado>> SimularProcesamientoParaleloWhenAllAsync(IEnumerable<Pet> mascotas);
    Task<SimulacionResultado> SimularAsignacionRapidaWhenAnyAsync(string nombreMascota);
    Task<string> SimularFlujoCompletoAtencionAsync(Pet mascota, Owner propietario);
}
