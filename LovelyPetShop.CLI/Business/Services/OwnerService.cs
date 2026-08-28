using LovelyPetShop.CLI.Domain.Entities;
using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.Business.Services;

public class OwnerService : IOwnerService
{
    private readonly IOwnerRepository _ownerRepository;
    private readonly IPetRepository _petRepository;

    public static readonly string[] ValidDocumentTypes = { "CC", "CE", "TI", "RC", "NIT", "PASAPORTE", "PEP", "PPT" };

    public OwnerService(IOwnerRepository ownerRepository, IPetRepository petRepository)
    {
        _ownerRepository = ownerRepository;
        _petRepository = petRepository;
    }

    public async Task<IEnumerable<Owner>> GetAllOwnersAsync()
    {
        var owners = (await _ownerRepository.GetAllAsync()).ToList();
        var allPets = (await _petRepository.GetAllAsync()).ToList();

        foreach (var owner in owners)
        {
            owner.Pets = allPets.Where(p =>
                string.Equals(p.OwnerDocumentNumber, owner.DocumentNumber, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(owner.Uuid) && string.Equals(p.OwnerUuid, owner.Uuid, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        return owners;
    }

    public async Task<Owner?> GetOwnerByUuidAsync(string uuid)
    {
        if (string.IsNullOrWhiteSpace(uuid)) return null;

        var owner = await _ownerRepository.GetByUuidAsync(uuid);
        if (owner != null)
        {
            var pets = await _petRepository.GetByOwnerDocumentNumberAsync(owner.DocumentNumber);
            owner.Pets = pets.ToList();
        }
        return owner;
    }

    public async Task<Owner?> GetOwnerByDocumentAsync(string documentNumber, string? documentType = null)
    {
        if (string.IsNullOrWhiteSpace(documentNumber)) return null;

        var owner = await _ownerRepository.GetByDocumentNumberAsync(documentNumber.Trim());
        if (owner != null)
        {
            if (!string.IsNullOrWhiteSpace(documentType) && !string.Equals(owner.DocumentType, documentType.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var pets = await _petRepository.GetByOwnerDocumentNumberAsync(owner.DocumentNumber);
            owner.Pets = pets.ToList();
        }
        return owner;
    }

    public async Task<(bool Success, string Message, string? CreatedUuid)> CreateOwnerAsync(string documentType, string documentNumber, string name, string phone, string email, string address)
    {
        if (string.IsNullOrWhiteSpace(documentType))
            return (false, "El tipo de documento es obligatorio.", null);

        documentType = documentType.Trim().ToUpper();
        if (!ValidDocumentTypes.Contains(documentType))
            return (false, $"Tipo de documento '{documentType}' no es válido. Tipos permitidos en Colombia: {string.Join(", ", ValidDocumentTypes)}.", null);

        if (string.IsNullOrWhiteSpace(documentNumber))
            return (false, "El número de documento es obligatorio.", null);

        if (string.IsNullOrWhiteSpace(name))
            return (false, "El nombre del propietario es obligatorio.", null);

        if (string.IsNullOrWhiteSpace(phone))
            return (false, "El teléfono de contacto es obligatorio.", null);

        var existingByDoc = await GetOwnerByDocumentAsync(documentNumber, documentType);
        if (existingByDoc != null)
            return (false, $"Ya existe un propietario registrado con {documentType} No. {documentNumber}.", null);

        var owner = new Owner
        {
            Uuid = Guid.NewGuid().ToString(),
            DocumentType = documentType,
            DocumentNumber = documentNumber.Trim(),
            Name = name.Trim(),
            Phone = phone.Trim(),
            Email = email?.Trim() ?? string.Empty,
            Address = address?.Trim() ?? string.Empty,
            CreatedAt = DateTime.Now
        };

        await _ownerRepository.AddAsync(owner);
        return (true, $"Propietario '{owner.Name}' registrado exitosamente ({owner.DocumentType} {owner.DocumentNumber} - UUID: {owner.Uuid}).", owner.Uuid);
    }

    public async Task<(bool Success, string Message)> UpdateOwnerAsync(string documentNumber, string? newDocumentType, string? newDocumentNumber, string? name, string? phone, string? email, string? address)
    {
        var owner = await GetOwnerByDocumentAsync(documentNumber);
        if (owner == null)
            return (false, $"No se encontró ningún propietario con el documento No. {documentNumber}.");

        string oldDocNumber = owner.DocumentNumber;

        if (!string.IsNullOrWhiteSpace(newDocumentType))
        {
            newDocumentType = newDocumentType.Trim().ToUpper();
            if (!ValidDocumentTypes.Contains(newDocumentType))
                return (false, $"Tipo de documento '{newDocumentType}' no es válido.");
            owner.DocumentType = newDocumentType;
        }

        if (!string.IsNullOrWhiteSpace(newDocumentNumber))
        {
            newDocumentNumber = newDocumentNumber.Trim();
            if (!string.Equals(oldDocNumber, newDocumentNumber, StringComparison.OrdinalIgnoreCase))
            {
                var existingOther = await GetOwnerByDocumentAsync(newDocumentNumber);
                if (existingOther != null)
                {
                    return (false, $"Ya existe otro propietario con el documento No. {newDocumentNumber}.");
                }
                owner.DocumentNumber = newDocumentNumber;

                // Actualizar número de documento en mascotas asociadas
                var pets = await _petRepository.GetByOwnerDocumentNumberAsync(oldDocNumber);
                foreach (var pet in pets)
                {
                    pet.OwnerDocumentNumber = newDocumentNumber;
                    await _petRepository.UpdateAsync(pet);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(name))
            owner.Name = name.Trim();

        if (!string.IsNullOrWhiteSpace(phone))
            owner.Phone = phone.Trim();

        if (email != null)
            owner.Email = email.Trim();

        if (address != null)
            owner.Address = address.Trim();

        await _ownerRepository.UpdateAsync(owner);
        return (true, $"Datos del propietario ({owner.DocumentType} {owner.DocumentNumber}) actualizados exitosamente.");
    }

    public async Task<(bool Success, string Message)> DeleteOwnerAsync(string documentNumber)
    {
        var owner = await GetOwnerByDocumentAsync(documentNumber);
        if (owner == null)
            return (false, $"No se encontró ningún propietario con el documento No. {documentNumber}.");

        var pets = await _petRepository.GetByOwnerDocumentNumberAsync(owner.DocumentNumber);
        if (pets.Any())
        {
            return (false, $"No se puede eliminar al propietario '{owner.Name}' (Doc: {owner.DocumentNumber}) porque tiene {pets.Count()} mascota(s) registrada(s). Elimine sus mascotas primero.");
        }

        var deleted = await _ownerRepository.DeleteByDocumentNumberAsync(owner.DocumentNumber);
        if (!deleted)
            return (false, "Ocurrió un error al intentar eliminar al propietario.");

        return (true, $"Propietario '{owner.Name}' (Doc: {owner.DocumentNumber}) eliminado exitosamente.");
    }
}
