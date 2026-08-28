using System.Text.Json.Serialization;
using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.Domain.Entities;

public class Owner : IRegistrable, INotificable
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("document_type")]
    public string DocumentType { get; set; } = "CC"; // CC, CE, TI, RC, NIT, PASAPORTE, PEP, PPT

    [JsonPropertyName("document_number")]
    public string DocumentNumber { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonIgnore]
    public List<Pet> Pets { get; set; } = new();

    public Owner()
    {
    }

    public Owner(string documentType, string documentNumber, string name, string phone, string email, string address)
    {
        Uuid = Guid.NewGuid().ToString();
        DocumentType = documentType;
        DocumentNumber = documentNumber;
        Name = name;
        Phone = phone;
        Email = email;
        Address = address;
        CreatedAt = DateTime.Now;
    }

    public string ObtenerResumenRegistro() =>
        $"[REGISTRO PROPIETARIO] {Name} | Documento: {DocumentType} {DocumentNumber} | Tel: {Phone} | Email: {Email} | Mascotas asociadas: {Pets.Count} | UUID: {Uuid}";

    public async Task<string> EnviarNotificacionAsync(string mensaje)
    {
        // Simulación asíncrona de envío de notificación (SMS / Email)
        await Task.Delay(150);
        return $"[NOTIFICACIÓN ENVIADA a {Name} ({Phone} / {Email})]: {mensaje}";
    }
}
