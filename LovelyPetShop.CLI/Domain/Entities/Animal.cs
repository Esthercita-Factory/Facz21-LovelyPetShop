namespace LovelyPetShop.CLI.Domain.Entities;

/// <summary>
/// Clase base abstracta que representa a cualquier animal en el sistema de la clínica veterinaria.
/// </summary>
public abstract class Animal
{
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public int Age { get; set; }
    public double Weight { get; set; }

    /// <summary>
    /// Método polimórfico para emitir sonido característico según la especie del animal.
    /// </summary>
    public abstract string EmitirSonido();
}
