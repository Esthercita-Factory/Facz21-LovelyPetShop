using System.Text.Json;
using LovelyPetShop.CLI.Domain.Entities;
using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.DataAccess.Repositories;

public class JsonOwnerRepository : IOwnerRepository
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonOwnerRepository(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "owners.json");
    }

    private async Task<List<Owner>> LoadDataAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Owner>();
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var list = await JsonSerializer.DeserializeAsync<List<Owner>>(stream, JsonOptions);
            return list ?? new List<Owner>();
        }
        catch (JsonException)
        {
            return new List<Owner>();
        }
    }

    private async Task SaveDataAsync(List<Owner> owners)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, owners, JsonOptions);
    }

    public async Task<IEnumerable<Owner>> GetAllAsync()
    {
        return await LoadDataAsync();
    }

    public async Task<Owner?> GetByUuidAsync(string uuid)
    {
        var owners = await LoadDataAsync();
        return owners.FirstOrDefault(o => string.Equals(o.Uuid, uuid, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Owner?> GetByDocumentNumberAsync(string documentNumber)
    {
        var owners = await LoadDataAsync();
        return owners.FirstOrDefault(o => string.Equals(o.DocumentNumber, documentNumber?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(Owner owner)
    {
        var owners = await LoadDataAsync();
        if (string.IsNullOrWhiteSpace(owner.Uuid))
        {
            owner.Uuid = Guid.NewGuid().ToString();
        }
        owners.Add(owner);
        await SaveDataAsync(owners);
    }

    public async Task UpdateAsync(Owner owner)
    {
        var owners = await LoadDataAsync();
        var index = owners.FindIndex(o => (!string.IsNullOrEmpty(owner.Uuid) && string.Equals(o.Uuid, owner.Uuid, StringComparison.OrdinalIgnoreCase)) ||
                                          string.Equals(o.DocumentNumber, owner.DocumentNumber, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            owners[index] = owner;
            await SaveDataAsync(owners);
        }
    }

    public async Task<bool> DeleteByDocumentNumberAsync(string documentNumber)
    {
        var owners = await LoadDataAsync();
        var count = owners.RemoveAll(o => string.Equals(o.DocumentNumber, documentNumber?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (count > 0)
        {
            await SaveDataAsync(owners);
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteByUuidAsync(string uuid)
    {
        var owners = await LoadDataAsync();
        var count = owners.RemoveAll(o => string.Equals(o.Uuid, uuid, StringComparison.OrdinalIgnoreCase));
        if (count > 0)
        {
            await SaveDataAsync(owners);
            return true;
        }
        return false;
    }
}
