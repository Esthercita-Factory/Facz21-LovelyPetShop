using LovelyPetShop.Domain.Entities;

namespace LovelyPetShop.Domain.Interfaces;

public interface IPetService
{
    Task<IEnumerable<Pet>> GetAllPetsAsync();
    Task<Pet?> GetPetByUuidAsync(string uuid);
    Task<IEnumerable<Pet>> GetPetsByOwnerDocumentAsync(string ownerDocumentNumber);
    Task<(bool Success, string Message, string? CreatedUuid)> CreatePetAsync(string name, string species, string breed, int age, double weight, string symptoms, string ownerDocumentNumber);
    Task<(bool Success, string Message, string? CreatedPetUuid, string? CreatedOwnerUuid)> CreatePetWithOwnerAsync(
        string petName, string species, string breed, int age, double weight, string symptoms,
        string docType, string docNumber, string ownerName, string ownerPhone, string ownerEmail, string ownerAddress);
    Task<(bool Success, string Message)> UpdatePetAsync(string petUuid, string? name, string? species, string? breed, int? age, double? weight, string? symptoms, string? ownerDocumentNumber);
    Task<(bool Success, string Message)> DeletePetAsync(string petUuid);
}
