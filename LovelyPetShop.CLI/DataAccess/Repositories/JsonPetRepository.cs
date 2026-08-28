using System.Text.Json;
using LovelyPetShop.CLI.Domain.Entities;
using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.DataAccess.Repositories;

public class JsonPetRepository : IPetRepository
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonPetRepository(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pets.json");
    }

    private async Task<List<Pet>> LoadDataAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Pet>();
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var list = await JsonSerializer.DeserializeAsync<List<Pet>>(stream, JsonOptions);
            return list ?? new List<Pet>();
        }
        catch (JsonException)
        {
            return new List<Pet>();
        }
    }

    private async Task SaveDataAsync(List<Pet> pets)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, pets, JsonOptions);
    }

    public async Task<IEnumerable<Pet>> GetAllAsync()
    {
        return await LoadDataAsync();
    }

    public async Task<Pet?> GetByUuidAsync(string uuid)
    {
        var pets = await LoadDataAsync();
        return pets.FirstOrDefault(p => string.Equals(p.Uuid, uuid, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Pet>> GetByOwnerDocumentNumberAsync(string ownerDocumentNumber)
    {
        var pets = await LoadDataAsync();
        return pets.Where(p => string.Equals(p.OwnerDocumentNumber, ownerDocumentNumber?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Pet>> GetByOwnerUuidAsync(string ownerUuid)
    {
        var pets = await LoadDataAsync();
        return pets.Where(p => string.Equals(p.OwnerUuid, ownerUuid, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(Pet pet)
    {
        var pets = await LoadDataAsync();
        if (string.IsNullOrWhiteSpace(pet.Uuid))
        {
            pet.Uuid = Guid.NewGuid().ToString();
        }
        pets.Add(pet);
        await SaveDataAsync(pets);
    }

    public async Task UpdateAsync(Pet pet)
    {
        var pets = await LoadDataAsync();
        var index = pets.FindIndex(p => string.Equals(p.Uuid, pet.Uuid, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            pets[index] = pet;
            await SaveDataAsync(pets);
        }
    }

    public async Task<bool> DeleteByUuidAsync(string uuid)
    {
        var pets = await LoadDataAsync();
        var count = pets.RemoveAll(p => string.Equals(p.Uuid, uuid, StringComparison.OrdinalIgnoreCase));
        if (count > 0)
        {
            await SaveDataAsync(pets);
            return true;
        }
        return false;
    }
}
