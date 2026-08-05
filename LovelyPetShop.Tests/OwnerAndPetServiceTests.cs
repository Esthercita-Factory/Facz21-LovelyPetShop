using LovelyPetShop.Business.Services;
using LovelyPetShop.Domain.Entities;
using LovelyPetShop.Domain.Interfaces;
using Xunit;

namespace LovelyPetShop.Tests;

public class FakeOwnerRepository : IOwnerRepository
{
    private readonly List<Owner> _owners = new();

    public Task<IEnumerable<Owner>> GetAllAsync() => Task.FromResult<IEnumerable<Owner>>(_owners);

    public Task<Owner?> GetByUuidAsync(string uuid) => Task.FromResult(_owners.FirstOrDefault(o => string.Equals(o.Uuid, uuid, StringComparison.OrdinalIgnoreCase)));

    public Task<Owner?> GetByDocumentNumberAsync(string documentNumber) => Task.FromResult(_owners.FirstOrDefault(o => string.Equals(o.DocumentNumber, documentNumber?.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Owner owner)
    {
        if (string.IsNullOrWhiteSpace(owner.Uuid)) owner.Uuid = Guid.NewGuid().ToString();
        _owners.Add(owner);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Owner owner)
    {
        var idx = _owners.FindIndex(o => string.Equals(o.Uuid, owner.Uuid, StringComparison.OrdinalIgnoreCase) || string.Equals(o.DocumentNumber, owner.DocumentNumber, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) _owners[idx] = owner;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteByDocumentNumberAsync(string documentNumber)
    {
        return Task.FromResult(_owners.RemoveAll(o => string.Equals(o.DocumentNumber, documentNumber?.Trim(), StringComparison.OrdinalIgnoreCase)) > 0);
    }

    public Task<bool> DeleteByUuidAsync(string uuid)
    {
        return Task.FromResult(_owners.RemoveAll(o => string.Equals(o.Uuid, uuid, StringComparison.OrdinalIgnoreCase)) > 0);
    }
}

public class FakePetRepository : IPetRepository
{
    private readonly List<Pet> _pets = new();

    public Task<IEnumerable<Pet>> GetAllAsync() => Task.FromResult<IEnumerable<Pet>>(_pets);

    public Task<Pet?> GetByUuidAsync(string uuid) => Task.FromResult(_pets.FirstOrDefault(p => string.Equals(p.Uuid, uuid, StringComparison.OrdinalIgnoreCase)));

    public Task<IEnumerable<Pet>> GetByOwnerDocumentNumberAsync(string ownerDocumentNumber) => Task.FromResult(_pets.Where(p => string.Equals(p.OwnerDocumentNumber, ownerDocumentNumber?.Trim(), StringComparison.OrdinalIgnoreCase)));

    public Task<IEnumerable<Pet>> GetByOwnerUuidAsync(string ownerUuid) => Task.FromResult(_pets.Where(p => string.Equals(p.OwnerUuid, ownerUuid, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Pet pet)
    {
        if (string.IsNullOrWhiteSpace(pet.Uuid)) pet.Uuid = Guid.NewGuid().ToString();
        _pets.Add(pet);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Pet pet)
    {
        var idx = _pets.FindIndex(p => string.Equals(p.Uuid, pet.Uuid, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0) _pets[idx] = pet;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteByUuidAsync(string uuid)
    {
        return Task.FromResult(_pets.RemoveAll(p => string.Equals(p.Uuid, uuid, StringComparison.OrdinalIgnoreCase)) > 0);
    }
}

public class OwnerAndPetServiceTests
{
    private readonly FakeOwnerRepository _ownerRepo;
    private readonly FakePetRepository _petRepo;
    private readonly OwnerService _ownerService;
    private readonly PetService _petService;

    public OwnerAndPetServiceTests()
    {
        _ownerRepo = new FakeOwnerRepository();
        _petRepo = new FakePetRepository();
        _ownerService = new OwnerService(_ownerRepo, _petRepo);
        _petService = new PetService(_petRepo, _ownerRepo, _ownerService);
    }

    [Fact]
    public async Task CreateOwner_WithValidDocument_GeneratesUuidAndStoresDocument()
    {
        var result = await _ownerService.CreateOwnerAsync("CC", "1018234567", "Carlos Pérez", "3001234567", "carlos@example.com", "Calle 10 #20-30");

        Assert.True(result.Success);
        Assert.NotNull(result.CreatedUuid);

        var owner = await _ownerService.GetOwnerByDocumentAsync("1018234567", "CC");
        Assert.NotNull(owner);
        Assert.Equal("CC", owner.DocumentType);
        Assert.Equal("1018234567", owner.DocumentNumber);
        Assert.False(string.IsNullOrWhiteSpace(owner.Uuid));
    }

    [Fact]
    public async Task CreateOwner_InvalidDocumentType_ReturnsError()
    {
        var result = await _ownerService.CreateOwnerAsync("INVALID_TYPE", "12345", "Test Owner", "3000000000", "test@test.com", "Address");
        Assert.False(result.Success);
        Assert.Contains("Invalid", result.Message);
    }

    [Fact]
    public async Task CreatePet_WithSpeciesAndBreed_StoresSuccessfully()
    {
        var ownerRes = await _ownerService.CreateOwnerAsync("PASSPORT", "PA987654", "María López", "3109876543", "maria@example.com", "Av Central");
        string ownerDocNumber = "PA987654";

        var petRes = await _petService.CreatePetAsync("Firulais", "Perro", "Criollo", 3, 12.5, "Chequeo general", ownerDocNumber);
        Assert.True(petRes.Success);
        Assert.NotNull(petRes.CreatedUuid);

        var pet = await _petService.GetPetByUuidAsync(petRes.CreatedUuid!);
        Assert.NotNull(pet);
        Assert.Equal("Perro", pet.Species);
        Assert.Equal("Criollo", pet.Breed);
        Assert.Equal(ownerDocNumber, pet.OwnerDocumentNumber);
        Assert.False(string.IsNullOrWhiteSpace(pet.Uuid));
    }

    [Fact]
    public async Task CreatePetWithOwnerCombined_RegistersBothOwnerAndPetInOneStep()
    {
        var combinedRes = await _petService.CreatePetWithOwnerAsync(
            "Michi", "Gato", "Persa", 2, 4.2, "Vacunación",
            "CE", "CE-456789", "Andrés Gómez", "3201112233", "andres@example.com", "Carrera 7");

        Assert.True(combinedRes.Success);
        Assert.NotNull(combinedRes.CreatedPetUuid);
        Assert.NotNull(combinedRes.CreatedOwnerUuid);

        var owner = await _ownerService.GetOwnerByDocumentAsync("CE-456789", "CE");
        var pet = await _petService.GetPetByUuidAsync(combinedRes.CreatedPetUuid!);

        Assert.NotNull(owner);
        Assert.Equal("CE", owner.DocumentType);
        Assert.Equal("CE-456789", owner.DocumentNumber);

        Assert.NotNull(pet);
        Assert.Equal("Michi", pet.Name);
        Assert.Equal("Persa", pet.Breed);
        Assert.Equal(owner.DocumentNumber, pet.OwnerDocumentNumber);
    }
}
