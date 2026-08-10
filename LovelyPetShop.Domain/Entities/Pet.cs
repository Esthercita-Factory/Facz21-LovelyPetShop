using System.Text.Json.Serialization;

namespace LovelyPetShop.Domain.Entities;

public class Pet
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("species")]
    public string Species { get; set; } = string.Empty;

    [JsonPropertyName("breed")]
    public string Breed { get; set; } = string.Empty;

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("weight")]
    public double Weight { get; set; }

    [JsonPropertyName("symptoms")]
    public string Symptoms { get; set; } = string.Empty;

    [JsonPropertyName("owner_document_number")]
    public string OwnerDocumentNumber { get; set; } = string.Empty;

    [JsonPropertyName("owner_uuid")]
    public string OwnerUuid { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Pet()
    {
    }

    public Pet(string name, string species, string breed, int age, double weight, string symptoms, string ownerDocumentNumber, string ownerUuid = "")
    {
        Uuid = Guid.NewGuid().ToString();
        Name = name;
        Species = species;
        Breed = breed;
        Age = age;
        Weight = weight;
        Symptoms = symptoms;
        OwnerDocumentNumber = ownerDocumentNumber;
        OwnerUuid = ownerUuid;
        CreatedAt = DateTime.Now;
    }
}
