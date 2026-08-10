using LovelyPetShop.Domain.Entities;

namespace LovelyPetShop.Domain.Interfaces;

public interface IOwnerService
{
    Task<IEnumerable<Owner>> GetAllOwnersAsync();
    Task<Owner?> GetOwnerByUuidAsync(string uuid);
    Task<Owner?> GetOwnerByDocumentAsync(string documentNumber, string? documentType = null);
    Task<(bool Success, string Message, string? CreatedUuid)> CreateOwnerAsync(string documentType, string documentNumber, string name, string phone, string email, string address);
    Task<(bool Success, string Message)> UpdateOwnerAsync(string documentNumber, string? newDocumentType, string? newDocumentNumber, string? name, string? phone, string? email, string? address);
    Task<(bool Success, string Message)> DeleteOwnerAsync(string documentNumber);
}
