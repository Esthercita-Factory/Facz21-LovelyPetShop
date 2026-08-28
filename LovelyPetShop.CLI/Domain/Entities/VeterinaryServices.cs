using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.Domain.Entities;

/// <summary>
/// Clase abstracta que define la estructura común de los servicios veterinarios.
/// </summary>
public abstract class ServicioVeterinario : IAtendible
{
    public string NombreServicio { get; set; } = string.Empty;
    public decimal CostoBase { get; set; }

    /// <summary>
    /// Método abstracto que define la atención clínica de una mascota específica.
    /// </summary>
    public abstract string Atender(Pet pet);
}

/// <summary>
/// Servicio concreto para consultas médicas veterinarias generales.
/// </summary>
public class ConsultaGeneral : ServicioVeterinario
{
    public string Motivo { get; set; } = "Revisión general y triaje";

    public ConsultaGeneral()
    {
        NombreServicio = "Consulta Médica General";
        CostoBase = 45000m;
    }

    public ConsultaGeneral(string motivo) : this()
    {
        Motivo = motivo;
    }

    public override string Atender(Pet pet)
    {
        return $"[CONSULTA] Atendiendo a '{pet.Name}' ({pet.Species} - {pet.Breed}, {pet.Age} años). " +
               $"Sonido registrado: '{pet.EmitirSonido()}'. Síntomas: '{pet.Symptoms}'. Costo: ${CostoBase:N0} COP.";
    }
}

/// <summary>
/// Servicio concreto para la aplicación de vacunas y desparasitación.
/// </summary>
public class Vacunacion : ServicioVeterinario
{
    public string TipoVacuna { get; set; } = "Antirrábica y Múltiple";

    public Vacunacion(string tipoVacuna = "Antirrábica y Múltiple")
    {
        NombreServicio = "Plan de Vacunación";
        CostoBase = 35000m;
        TipoVacuna = tipoVacuna;
    }

    public override string Atender(Pet pet)
    {
        return $"[VACUNACIÓN] Aplicando vacuna '{TipoVacuna}' a '{pet.Name}' ({pet.Species}). " +
               $"Reacción sonora: '{pet.EmitirSonido()}'. Costo: ${CostoBase:N0} COP.";
    }
}
