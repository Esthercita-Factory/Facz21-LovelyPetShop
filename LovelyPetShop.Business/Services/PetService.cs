using LovelyPetShop.Domain.Entities;
using LovelyPetShop.Domain.Interfaces;

namespace LovelyPetShop.Business.Services;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepository;
    private readonly IOwnerRepository _ownerRepository;
    private readonly IOwnerService _ownerService;

    public PetService(IPetRepository petRepository, IOwnerRepository ownerRepository, IOwnerService ownerService)
    {
        _petRepository = petRepository;
        _ownerRepository = ownerRepository;
        _ownerService = ownerService;
    }

    public async Task<IEnumerable<Pet>> GetAllPetsAsync()
    {
        return await _petRepository.GetAllAsync();
    }

    public async Task<Pet?> GetPetByUuidAsync(string uuid)
    {
        if (string.IsNullOrWhiteSpace(uuid)) return null;
        return await _petRepository.GetByUuidAsync(uuid.Trim());
    }

    public async Task<IEnumerable<Pet>> GetPetsByOwnerDocumentAsync(string ownerDocumentNumber)
    {
        if (string.IsNullOrWhiteSpace(ownerDocumentNumber)) return Enumerable.Empty<Pet>();
        return await _petRepository.GetByOwnerDocumentNumberAsync(ownerDocumentNumber.Trim());
    }

    public async Task<(bool Success, string Message, string? CreatedUuid)> CreatePetAsync(string name, string species, string breed, int age, double weight, string symptoms, string ownerDocumentNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, "Pet name is required.", null);

        if (string.IsNullOrWhiteSpace(species))
            return (false, "Species is required.", null);

        if (string.IsNullOrWhiteSpace(breed))
            breed = "Mixed Breed";

        if (age < 0)
            return (false, "Age cannot be negative.", null);

        if (weight < 0)
            return (false, "Weight cannot be negative.", null);

        if (string.IsNullOrWhiteSpace(ownerDocumentNumber))
            return (false, "Owner document number is required.", null);

        var owner = await _ownerService.GetOwnerByDocumentAsync(ownerDocumentNumber.Trim());
        if (owner == null)
            return (false, $"Error: No registered owner found with document No. '{ownerDocumentNumber}'.", null);

        var pet = new Pet
        {
            Uuid = Guid.NewGuid().ToString(),
            Name = name.Trim(),
            Species = species.Trim(),
            Breed = breed.Trim(),
            Age = age,
            Weight = weight,
            Symptoms = symptoms?.Trim() ?? string.Empty,
            OwnerDocumentNumber = owner.DocumentNumber,
            OwnerUuid = owner.Uuid,
            CreatedAt = DateTime.Now
        };

        await _petRepository.AddAsync(pet);
        return (true, $"Pet '{pet.Name}' (Species: {pet.Species}, Breed: {pet.Breed}) successfully registered for owner '{owner.Name}' (Doc: {owner.DocumentNumber}, Pet UUID: {pet.Uuid}).", pet.Uuid);
    }

    public async Task<(bool Success, string Message, string? CreatedPetUuid, string? CreatedOwnerUuid)> CreatePetWithOwnerAsync(
        string petName, string species, string breed, int age, double weight, string symptoms,
        string docType, string docNumber, string ownerName, string ownerPhone, string ownerEmail, string ownerAddress)
    {
        string ownerDocNum;
        string? ownerUuid = null;

        var existingOwner = await _ownerService.GetOwnerByDocumentAsync(docNumber, docType);
        if (existingOwner != null)
        {
            ownerDocNum = existingOwner.DocumentNumber;
            ownerUuid = existingOwner.Uuid;
        }
        else
        {
            var ownerResult = await _ownerService.CreateOwnerAsync(docType, docNumber, ownerName, ownerPhone, ownerEmail, ownerAddress);
            if (!ownerResult.Success || string.IsNullOrEmpty(ownerResult.CreatedUuid))
            {
                return (false, $"Error registering owner: {ownerResult.Message}", null, null);
            }
            ownerDocNum = docNumber.Trim();
            ownerUuid = ownerResult.CreatedUuid;
        }

        var petResult = await CreatePetAsync(petName, species, breed, age, weight, symptoms, ownerDocNum);
        if (!petResult.Success)
        {
            return (false, petResult.Message, null, ownerUuid);
        }

        return (true, $"Pet '{petName}' and owner registered successfully. (Pet UUID: {petResult.CreatedUuid}, Owner Doc: {ownerDocNum})", petResult.CreatedUuid, ownerUuid);
    }

    public async Task<(bool Success, string Message)> UpdatePetAsync(string petUuid, string? name, string? species, string? breed, int? age, double? weight, string? symptoms, string? ownerDocumentNumber)
    {
        var pet = await GetPetByUuidAsync(petUuid);
        if (pet == null)
            return (false, $"No pet found with UUID '{petUuid}'.");

        if (!string.IsNullOrWhiteSpace(ownerDocumentNumber) && !string.Equals(ownerDocumentNumber.Trim(), pet.OwnerDocumentNumber, StringComparison.OrdinalIgnoreCase))
        {
            var newOwner = await _ownerService.GetOwnerByDocumentAsync(ownerDocumentNumber.Trim());
            if (newOwner == null)
                return (false, $"No registered owner found with document No. '{ownerDocumentNumber}'.");

            pet.OwnerDocumentNumber = newOwner.DocumentNumber;
            pet.OwnerUuid = newOwner.Uuid;
        }

        if (!string.IsNullOrWhiteSpace(name))
            pet.Name = name.Trim();

        if (!string.IsNullOrWhiteSpace(species))
            pet.Species = species.Trim();

        if (!string.IsNullOrWhiteSpace(breed))
            pet.Breed = breed.Trim();

        if (age.HasValue)
        {
            if (age.Value < 0)
                return (false, "Age cannot be negative.");
            pet.Age = age.Value;
        }

        if (weight.HasValue)
        {
            if (weight.Value < 0)
                return (false, "Weight cannot be negative.");
            pet.Weight = weight.Value;
        }

        if (symptoms != null)
            pet.Symptoms = symptoms.Trim();

        await _petRepository.UpdateAsync(pet);
        return (true, $"Pet '{pet.Name}' (UUID: {pet.Uuid}) data updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeletePetAsync(string petUuid)
    {
        var pet = await GetPetByUuidAsync(petUuid);
        if (pet == null)
            return (false, $"No pet found with UUID '{petUuid}'.");

        var deleted = await _petRepository.DeleteByUuidAsync(pet.Uuid);
        if (!deleted)
            return (false, "An error occurred while attempting to delete the pet.");

        return (true, $"Pet '{pet.Name}' (UUID: {pet.Uuid}) removed from the system.");
    }
}
