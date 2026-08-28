namespace LovelyPetShop.CLI.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando una mascota solicitada no se encuentra en el sistema.
/// </summary>
public class MascotaNoEncontradaException : Exception
{
    public MascotaNoEncontradaException(string message) : base(message) { }
    public MascotaNoEncontradaException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Excepción lanzada cuando un propietario solicitado no se encuentra en el sistema.
/// </summary>
public class PropietarioNoEncontradoException : Exception
{
    public PropietarioNoEncontradoException(string message) : base(message) { }
    public PropietarioNoEncontradoException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Excepción lanzada cuando se violan reglas de validación o del dominio de la clínica.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string message) : base(message) { }
    public ReglaNegocioException(string message, Exception innerException) : base(message, innerException) { }
}
