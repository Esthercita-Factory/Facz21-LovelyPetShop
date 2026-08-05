using LovelyPetShop.Domain.Entities;
using LovelyPetShop.Domain.Interfaces;

namespace LovelyPetShop.Business.Services;

public class OwnerService : IOwnerService
{
    private readonly IOwnerRepository _ownerRepository;
    private readonly IPetRepository _petRepository;

    public static readonly string[] ValidDocumentTypes = { "CC", "CE", "TI", "PASSPORT", "NIT", "PEP" };

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
            return (false, "Document type is required.", null);

        documentType = documentType.Trim().ToUpper();
        if (!ValidDocumentTypes.Contains(documentType))
            return (false, $"Invalid document type '{documentType}'. Allowed types: {string.Join(", ", ValidDocumentTypes)}", null);

        if (string.IsNullOrWhiteSpace(documentNumber))
            return (false, "Document number is required.", null);

        if (string.IsNullOrWhiteSpace(name))
            return (false, "Owner name is required.", null);

        if (string.IsNullOrWhiteSpace(phone))
            return (false, "Contact phone is required.", null);

        var existingByDoc = await GetOwnerByDocumentAsync(documentNumber, documentType);
        if (existingByDoc != null)
            return (false, $"An owner with {documentType} No. {documentNumber} is already registered.", null);

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
        return (true, $"Owner '{owner.Name}' registered successfully ({owner.DocumentType} {owner.DocumentNumber} - UUID: {owner.Uuid}).", owner.Uuid);
    }

    public async Task<(bool Success, string Message)> UpdateOwnerAsync(string documentNumber, string? newDocumentType, string? newDocumentNumber, string? name, string? phone, string? email, string? address)
    {
        var owner = await GetOwnerByDocumentAsync(documentNumber);
        if (owner == null)
            return (false, $"No owner found with document No. {documentNumber}.");

        string oldDocNumber = owner.DocumentNumber;

        if (!string.IsNullOrWhiteSpace(newDocumentType))
        {
            newDocumentType = newDocumentType.Trim().ToUpper();
            if (!ValidDocumentTypes.Contains(newDocumentType))
                return (false, $"Invalid document type '{newDocumentType}'.");
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
                    return (false, $"Another owner with document No. {newDocumentNumber} already exists.");
                }
                owner.DocumentNumber = newDocumentNumber;

                // Update owner document number in pets
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
        return (true, $"Owner ({owner.DocumentType} {owner.DocumentNumber}) data updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteOwnerAsync(string documentNumber)
    {
        var owner = await GetOwnerByDocumentAsync(documentNumber);
        if (owner == null)
            return (false, $"No owner found with document No. {documentNumber}.");

        var pets = await _petRepository.GetByOwnerDocumentNumberAsync(owner.DocumentNumber);
        if (pets.Any())
        {
            return (false, $"Cannot delete owner '{owner.Name}' (Doc: {owner.DocumentNumber}) because they have {pets.Count()} registered pet(s). Delete their pets first.");
        }

        var deleted = await _ownerRepository.DeleteByDocumentNumberAsync(owner.DocumentNumber);
        if (!deleted)
            return (false, "An error occurred while attempting to delete the owner.");

        return (true, $"Owner '{owner.Name}' (Doc: {owner.DocumentNumber}) deleted successfully.");
    }
}
