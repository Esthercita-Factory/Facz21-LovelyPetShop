using LovelyPetShop.CLI.Domain.Entities;

namespace LovelyPetShop.CLI.Domain.Interfaces;

public interface IOwnerRepository
{
    Task<IEnumerable<Owner>> GetAllAsync();
    Task<Owner?> GetByUuidAsync(string uuid);
    Task<Owner?> GetByDocumentNumberAsync(string documentNumber);
    Task AddAsync(Owner owner);
    Task UpdateAsync(Owner owner);
    Task<bool> DeleteByDocumentNumberAsync(string documentNumber);
    Task<bool> DeleteByUuidAsync(string uuid);
}
