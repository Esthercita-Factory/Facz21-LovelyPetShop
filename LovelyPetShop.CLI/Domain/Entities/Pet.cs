using System.Text.Json.Serialization;
using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.Domain.Entities;

public class Pet : Animal, IRegistrable
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public new string Name
    {
        get => base.Name;
        set => base.Name = value;
    }

    [JsonPropertyName("species")]
    public new string Species
    {
        get => base.Species;
        set => base.Species = value;
    }

    [JsonPropertyName("breed")]
    public string Breed { get; set; } = string.Empty;

    [JsonPropertyName("age")]
    public new int Age
    {
        get => base.Age;
        set => base.Age = value;
    }

    [JsonPropertyName("weight")]
    public new double Weight
    {
        get => base.Weight;
        set => base.Weight = value;
    }

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

    public override string EmitirSonido() => Species.ToLower() switch
    {
        "perro" or "canino" => "¡Guau guau!",
        "gato" or "felino" => "¡Miau miau!",
        "ave" or "pájaro" or "loro" or "canario" => "¡Pío pío / Kraaa!",
        "conejo" or "roedor" or "hamster" => "¡Chii chii!",
        _ => "¡Sonido característico de animal!"
    };

    public string ObtenerResumenRegistro() =>
        $"[REGISTRO MASCOTA] {Name} ({Species} - {Breed}) | Edad: {Age}a | Peso: {Weight}kg | Síntomas: {Symptoms} | Doc Dueño: {OwnerDocumentNumber} | UUID: {Uuid}";
}
