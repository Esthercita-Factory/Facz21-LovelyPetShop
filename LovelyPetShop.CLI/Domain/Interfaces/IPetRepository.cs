using LovelyPetShop.CLI.Domain.Entities;

namespace LovelyPetShop.CLI.Domain.Interfaces;

public interface IPetRepository
{
    Task<IEnumerable<Pet>> GetAllAsync();
    Task<Pet?> GetByUuidAsync(string uuid);
    Task<IEnumerable<Pet>> GetByOwnerDocumentNumberAsync(string ownerDocumentNumber);
    Task<IEnumerable<Pet>> GetByOwnerUuidAsync(string ownerUuid);
    Task AddAsync(Pet pet);
    Task UpdateAsync(Pet pet);
    Task<bool> DeleteByUuidAsync(string uuid);
}
