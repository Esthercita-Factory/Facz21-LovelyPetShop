using LovelyPetShop.Business.Services;
using LovelyPetShop.Domain.Entities;
using LovelyPetShop.Domain.Interfaces;

namespace LovelyPetShop.CLI.UI;

public class ConsoleMenu
{
    private readonly IOwnerService _ownerService;
    private readonly IPetService _petService;
    private const int PageSize = 5;

    public ConsoleMenu(IOwnerService ownerService, IPetService petService)
    {
        _ownerService = ownerService;
        _petService = petService;
    }

    public async Task RunAsync()
    {
        bool running = true;

        while (running)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Owner Management");
            Console.WriteLine("2. Pet Management");
            Console.WriteLine("3. Register Pet & Owner (1-Step Quick Register)");
            Console.WriteLine("4. General Report (Owners & Pets)");
            Console.WriteLine("5. Exit");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nSelect an option: ");
            Console.ResetColor();

            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await OwnerSubMenuAsync();
                    break;
                case "2":
                    await PetSubMenuAsync();
                    break;
                case "3":
                    await CreatePetWithOwnerCombinedAsync();
                    Pause();
                    break;
                case "4":
                    await DisplayFullReportAsync();
                    break;
                case "5":
                    running = false;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Thank you for using LovelyPetShop CLI. Goodbye!");
                    Console.ResetColor();
                    break;
                default:
                    PrintError("Invalid option. Please try again.");
                    Pause();
                    break;
            }
        }
    }

    private static void DisplayHeader()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("+----------------------------------------------------------+");
        Console.WriteLine("|               LOVELY PET SHOP - VETERINARY               |");
        Console.WriteLine("|             Layered Management Architecture              |");
        Console.WriteLine("+----------------------------------------------------------+");
        Console.ResetColor();
    }

    #region Owner Submenu
    private async Task OwnerSubMenuAsync()
    {
        bool inSubMenu = true;
        while (inSubMenu)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- OWNER MANAGEMENT ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Register new owner");
            Console.WriteLine("2. Search owner by Document Number");
            Console.WriteLine("3. List all owners (Paginated)");
            Console.WriteLine("4. Update owner details");
            Console.WriteLine("5. Delete owner");
            Console.WriteLine("6. Back to main menu");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nSelect option: ");
            Console.ResetColor();

            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await CreateOwnerAsync();
                    Pause();
                    break;
                case "2":
                    await ReadOwnerByDocumentAsync();
                    Pause();
                    break;
                case "3":
                    await PaginateOwnersAsync();
                    break;
                case "4":
                    await UpdateOwnerAsync();
                    Pause();
                    break;
                case "5":
                    await DeleteOwnerAsync();
                    Pause();
                    break;
                case "6":
                    inSubMenu = false;
                    break;
                default:
                    PrintError("Invalid option.");
                    Pause();
                    break;
            }
        }
    }

    private async Task CreateOwnerAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[+] REGISTER NEW OWNER");
        Console.ResetColor();

        string docType = PromptDocumentType();

        Console.Write("Document Number: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Full Name: ");
        string name = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Contact Phone: ");
        string phone = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Email: ");
        string email = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Address: ");
        string address = Console.ReadLine()?.Trim() ?? string.Empty;

        var result = await _ownerService.CreateOwnerAsync(docType, docNum, name, phone, email, address);
        if (result.Success)
            PrintSuccess(result.Message);
        else
            PrintError(result.Message);
    }

    private async Task ReadOwnerByDocumentAsync()
    {
        Console.Write("Owner Document Number: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var owner = await _ownerService.GetOwnerByDocumentAsync(docNum);
        if (owner == null)
        {
            PrintError($"No owner found with document number '{docNum}'.");
            return;
        }

        DisplayOwnerDetails(owner);
    }

    private static void DisplayOwnerDetails(Owner owner)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[INFO] Owner Details");
        Console.WriteLine($"   UUID:           {owner.Uuid}");
        Console.WriteLine($"   Doc Type:       {owner.DocumentType}");
        Console.WriteLine($"   Doc No.:        {owner.DocumentNumber}");
        Console.WriteLine($"   Name:           {owner.Name}");
        Console.WriteLine($"   Phone:          {owner.Phone}");
        Console.WriteLine($"   Email:          {owner.Email}");
        Console.WriteLine($"   Address:        {owner.Address}");
        Console.WriteLine($"   Registration:   {owner.CreatedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"   Registered Pets: {owner.Pets.Count}");

        foreach (var p in owner.Pets)
        {
            Console.WriteLine($"     - Pet [{p.Name}] | Species: {p.Species} | Breed: {p.Breed} | Age: {p.Age}y | UUID: {p.Uuid}");
        }
        Console.ResetColor();
    }

    private async Task PaginateOwnersAsync()
    {
        var owners = (await _ownerService.GetAllOwnersAsync()).ToList();
        if (!owners.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No registered owners in the system.");
            Console.ResetColor();
            Pause();
            return;
        }

        int currentPage = 1;
        int totalPages = (int)Math.Ceiling(owners.Count / (double)PageSize);
        bool browsing = true;

        while (browsing)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"--- PAGINATED OWNERS LIST (Page {currentPage} of {totalPages} | Total: {owners.Count}) ---");
            Console.WriteLine("{0,-4} | {1,-10} | {2,-14} | {3,-20} | {4,-12} | {5,-36}", "#", "Doc Type", "Doc No.", "Name", "Phone", "UUID");
            Console.WriteLine(new string('-', 98));
            Console.ResetColor();

            var pageOwners = owners.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();
            int itemIndex = (currentPage - 1) * PageSize + 1;

            foreach (var o in pageOwners)
            {
                Console.WriteLine("{0,-4} | {1,-10} | {2,-14} | {3,-20} | {4,-12} | {5,-36}",
                    itemIndex++, o.DocumentType, Truncate(o.DocumentNumber, 14), Truncate(o.Name, 20), Truncate(o.Phone, 12), o.Uuid);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[N] Next page | [P] Previous page | [Q] Back to menu");
            Console.Write("Select option: ");
            Console.ResetColor();

            string nav = Console.ReadLine()?.Trim().ToUpper() ?? string.Empty;
            if (nav == "N" && currentPage < totalPages)
            {
                currentPage++;
            }
            else if (nav == "P" && currentPage > 1)
            {
                currentPage--;
            }
            else if (nav == "Q" || nav == "V" || nav == "B")
            {
                browsing = false;
            }
        }
    }

    private async Task UpdateOwnerAsync()
    {
        Console.Write("Enter Document Number of owner to update: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var existing = await _ownerService.GetOwnerByDocumentAsync(docNum);
        if (existing == null)
        {
            PrintError($"No owner found with document No. '{docNum}'.");
            return;
        }

        Console.WriteLine($"Updating details for {existing.Name} (leave blank to keep current value):");

        Console.Write($"New Doc Type [{existing.DocumentType}] (Enter to keep): ");
        string? newDocType = Console.ReadLine()?.Trim();
        newDocType = string.IsNullOrEmpty(newDocType) ? null : newDocType;

        Console.Write($"New Doc No. [{existing.DocumentNumber}]: ");
        string? newDocNum = Console.ReadLine()?.Trim();
        newDocNum = string.IsNullOrEmpty(newDocNum) ? null : newDocNum;

        Console.Write($"New Name [{existing.Name}]: ");
        string? name = Console.ReadLine()?.Trim();
        name = string.IsNullOrEmpty(name) ? null : name;

        Console.Write($"New Phone [{existing.Phone}]: ");
        string? phone = Console.ReadLine()?.Trim();
        phone = string.IsNullOrEmpty(phone) ? null : phone;

        Console.Write($"New Email [{existing.Email}]: ");
        string? email = Console.ReadLine()?.Trim();
        email = string.IsNullOrEmpty(email) ? null : email;

        Console.Write($"New Address [{existing.Address}]: ");
        string? address = Console.ReadLine()?.Trim();
        address = string.IsNullOrEmpty(address) ? null : address;

        var result = await _ownerService.UpdateOwnerAsync(docNum, newDocType, newDocNum, name, phone, email, address);
        if (result.Success)
            PrintSuccess(result.Message);
        else
            PrintError(result.Message);
    }

    private async Task DeleteOwnerAsync()
    {
        Console.Write("Enter Document Number of owner to delete: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var result = await _ownerService.DeleteOwnerAsync(docNum);
        if (result.Success)
            PrintSuccess(result.Message);
        else
            PrintError(result.Message);
    }
    #endregion

    #region Pet Submenu
    private async Task PetSubMenuAsync()
    {
        bool inSubMenu = true;
        while (inSubMenu)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- PET MANAGEMENT ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Register new pet (for existing owner)");
            Console.WriteLine("2. Register pet and new owner (1-step)");
            Console.WriteLine("3. Search pet by UUID");
            Console.WriteLine("4. List pets by Owner Document");
            Console.WriteLine("5. List all pets (Paginated)");
            Console.WriteLine("6. Update pet");
            Console.WriteLine("7. Delete pet");
            Console.WriteLine("8. Back to main menu");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nSelect option: ");
            Console.ResetColor();

            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await CreatePetAsync();
                    Pause();
                    break;
                case "2":
                    await CreatePetWithOwnerCombinedAsync();
                    Pause();
                    break;
                case "3":
                    await ReadPetByUuidAsync();
                    Pause();
                    break;
                case "4":
                    await ListPetsByOwnerDocumentAsync();
                    Pause();
                    break;
                case "5":
                    await PaginatePetsAsync();
                    break;
                case "6":
                    await UpdatePetAsync();
                    Pause();
                    break;
                case "7":
                    await DeletePetAsync();
                    Pause();
                    break;
                case "8":
                    inSubMenu = false;
                    break;
                default:
                    PrintError("Invalid option.");
                    Pause();
                    break;
            }
        }
    }

    private async Task CreatePetAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[+] REGISTER NEW PET");
        Console.ResetColor();

        Console.Write("Owner Document Number: ");
        string ownerDocNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var owner = await _ownerService.GetOwnerByDocumentAsync(ownerDocNum);
        if (owner == null)
        {
            PrintError($"No owner registered with document No. '{ownerDocNum}'. You can register both using combined registration.");
            return;
        }

        Console.WriteLine($"Selected owner: {owner.Name} ({owner.DocumentType} {owner.DocumentNumber})");

        Console.Write("Pet Name: ");
        string name = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Species (e.g. Dog, Cat, Bird): ");
        string species = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Breed (e.g. Mutt, Labrador, Mixed): ");
        string breed = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Age (years): ");
        if (!int.TryParse(Console.ReadLine(), out int age))
        {
            PrintError("Invalid age.");
            return;
        }

        Console.Write("Weight (kg): ");
        if (!double.TryParse(Console.ReadLine(), out double weight))
        {
            PrintError("Invalid weight.");
            return;
        }

        Console.Write("Symptoms / Reason for visit: ");
        string symptoms = Console.ReadLine()?.Trim() ?? string.Empty;

        var result = await _petService.CreatePetAsync(name, species, breed, age, weight, symptoms, ownerDocNum);
        if (result.Success)
            PrintSuccess(result.Message);
        else
            PrintError(result.Message);
    }

    private async Task CreatePetWithOwnerCombinedAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[+] COMBINED REGISTRATION: PET + OWNER");
        Console.ResetColor();

        Console.WriteLine("\n--- OWNER DETAILS ---");
        string docType = PromptDocumentType();

        Console.Write("Owner Document Number: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Owner Full Name: ");
        string ownerName = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Contact Phone: ");
        string ownerPhone = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Email: ");
        string ownerEmail = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Address: ");
        string ownerAddress = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.WriteLine("\n--- PET DETAILS ---");
        Console.Write("Pet Name: ");
        string petName = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Species (e.g. Dog, Cat): ");
        string species = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Breed (e.g. Mutt, Poodle): ");
        string breed = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Age (years): ");
        if (!int.TryParse(Console.ReadLine(), out int age))
        {
            PrintError("Invalid age.");
            return;
        }

        Console.Write("Weight (kg): ");
        if (!double.TryParse(Console.ReadLine(), out double weight))
        {
            PrintError("Invalid weight.");
            return;
        }

        Console.Write("Symptoms / Reason for visit: ");
        string symptoms = Console.ReadLine()?.Trim() ?? string.Empty;

        var result = await _petService.CreatePetWithOwnerAsync(
            petName, species, breed, age, weight, symptoms,
            docType, docNum, ownerName, ownerPhone, ownerEmail, ownerAddress);

        if (result.Success)
            PrintSuccess(result.Message);
        else
            PrintError(result.Message);
    }

    private async Task ReadPetByUuidAsync()
    {
        Console.Write("Enter Pet UUID: ");
        string uuid = Console.ReadLine()?.Trim() ?? string.Empty;

        var pet = await _petService.GetPetByUuidAsync(uuid);
        if (pet == null)
        {
            PrintError($"No pet found with UUID '{uuid}'.");
            return;
        }

        var owner = await _ownerService.GetOwnerByDocumentAsync(pet.OwnerDocumentNumber);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[INFO] Pet Details");
        Console.WriteLine($"   UUID:              {pet.Uuid}");
        Console.WriteLine($"   Name:              {pet.Name}");
        Console.WriteLine($"   Species:           {pet.Species}");
        Console.WriteLine($"   Breed:             {pet.Breed}");
        Console.WriteLine($"   Age:               {pet.Age} years");
        Console.WriteLine($"   Weight:            {pet.Weight} kg");
        Console.WriteLine($"   Symptoms:          {pet.Symptoms}");
        Console.WriteLine($"   Responsible Owner: {owner?.Name ?? "Unknown"} (Doc No. {pet.OwnerDocumentNumber})");
        Console.WriteLine($"   Registration Date: {pet.CreatedAt:yyyy-MM-dd HH:mm}");
        Console.ResetColor();
    }

    private async Task ListPetsByOwnerDocumentAsync()
    {
        Console.Write("Enter Owner Document Number: ");
        string ownerDocNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var owner = await _ownerService.GetOwnerByDocumentAsync(ownerDocNum);
        if (owner == null)
        {
            PrintError($"No owner found with document No. '{ownerDocNum}'.");
            return;
        }

        var pets = (await _petService.GetPetsByOwnerDocumentAsync(ownerDocNum)).ToList();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nPets belonging to '{owner.Name}' ({owner.DocumentType} No. {owner.DocumentNumber}):");
        Console.ResetColor();

        if (!pets.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("This owner has no registered pets.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("{0,-4} | {1,-15} | {2,-10} | {3,-12} | {4,-6} | {5,-8} | {6,-36}", "#", "Name", "Species", "Breed", "Age", "Weight", "Pet UUID");
        Console.WriteLine(new string('-', 100));
        int idx = 1;
        foreach (var p in pets)
        {
            Console.WriteLine("{0,-4} | {1,-15} | {2,-10} | {3,-12} | {4,-6} | {5,-8} | {6,-36}",
                idx++, Truncate(p.Name, 15), Truncate(p.Species, 10), Truncate(p.Breed, 12), $"{p.Age}y", $"{p.Weight}kg", p.Uuid);
        }
    }

    private async Task PaginatePetsAsync()
    {
        var pets = (await _petService.GetAllPetsAsync()).ToList();
        if (!pets.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No registered pets in the clinic.");
            Console.ResetColor();
            Pause();
            return;
        }

        int currentPage = 1;
        int totalPages = (int)Math.Ceiling(pets.Count / (double)PageSize);
        bool browsing = true;

        while (browsing)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"--- PAGINATED PETS LIST (Page {currentPage} of {totalPages} | Total: {pets.Count}) ---");
            Console.WriteLine("{0,-4} | {1,-15} | {2,-10} | {3,-12} | {4,-6} | {5,-8} | {6,-14} | {7,-36}", "#", "Name", "Species", "Breed", "Age", "Weight", "Owner Doc", "Pet UUID");
            Console.WriteLine(new string('-', 116));
            Console.ResetColor();

            var pagePets = pets.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();
            int itemIndex = (currentPage - 1) * PageSize + 1;

            foreach (var p in pagePets)
            {
                Console.WriteLine("{0,-4} | {1,-15} | {2,-10} | {3,-12} | {4,-6} | {5,-8} | {6,-14} | {7,-36}",
                    itemIndex++, Truncate(p.Name, 15), Truncate(p.Species, 10), Truncate(p.Breed, 12), $"{p.Age}y", $"{p.Weight}kg", Truncate(p.OwnerDocumentNumber, 14), p.Uuid);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[N] Next page | [P] Previous page | [Q] Back to menu");
            Console.Write("Select option: ");
            Console.ResetColor();

            string nav = Console.ReadLine()?.Trim().ToUpper() ?? string.Empty;
            if (nav == "N" && currentPage < totalPages)
            {
                currentPage++;
            }
            else if (nav == "P" && currentPage > 1)
            {
                currentPage--;
            }
            else if (nav == "Q" || nav == "V" || nav == "B")
            {
                browsing = false;
            }
        }
    }

    private async Task UpdatePetAsync()
    {
        Console.Write("Enter UUID of pet to update: ");
        string uuid = Console.ReadLine()?.Trim() ?? string.Empty;

        var existing = await _petService.GetPetByUuidAsync(uuid);
        if (existing == null)
        {
            PrintError($"No pet found with UUID '{uuid}'.");
            return;
        }

        Console.WriteLine($"Updating details for {existing.Name} (leave blank to keep current value):");

        Console.Write($"Name [{existing.Name}]: ");
        string? name = Console.ReadLine()?.Trim();
        name = string.IsNullOrEmpty(name) ? null : name;

        Console.Write($"Species [{existing.Species}]: ");
        string? species = Console.ReadLine()?.Trim();
        species = string.IsNullOrEmpty(species) ? null : species;

        Console.Write($"Breed [{existing.Breed}]: ");
        string? breed = Console.ReadLine()?.Trim();
        breed = string.IsNullOrEmpty(breed) ? null : breed;

        Console.Write($"Age [{existing.Age}]: ");
        string? inputAge = Console.ReadLine()?.Trim();
        int? age = string.IsNullOrEmpty(inputAge) ? null : int.TryParse(inputAge, out int a) ? a : null;

        Console.Write($"Weight [{existing.Weight} kg]: ");
        string? inputWeight = Console.ReadLine()?.Trim();
        double? weight = string.IsNullOrEmpty(inputWeight) ? null : double.TryParse(inputWeight, out double w) ? w : null;

        Console.Write($"Symptoms [{existing.Symptoms}]: ");
        string? symptoms = Console.ReadLine()?.Trim();
        symptoms = string.IsNullOrEmpty(symptoms) ? null : symptoms;

        Console.Write($"New Owner Document [{existing.OwnerDocumentNumber}]: ");
        string? ownerDocNum = Console.ReadLine()?.Trim();
        ownerDocNum = string.IsNullOrEmpty(ownerDocNum) ? null : ownerDocNum;

        var result = await _petService.UpdatePetAsync(uuid, name, species, breed, age, weight, symptoms, ownerDocNum);
        if (result.Success)
            PrintSuccess(result.Message);
        else
            PrintError(result.Message);
    }

    private async Task DeletePetAsync()
    {
        Console.Write("Enter UUID of pet to delete: ");
        string uuid = Console.ReadLine()?.Trim() ?? string.Empty;

        var result = await _petService.DeletePetAsync(uuid);
        if (result.Success)
            PrintSuccess(result.Message);
        else
            PrintError(result.Message);
    }
    #endregion

    #region Full Report
    private async Task DisplayFullReportAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==========================================================");
        Console.WriteLine("       GENERAL REPORT: OWNERS AND PETS                    ");
        Console.WriteLine("==========================================================");
        Console.ResetColor();

        var owners = (await _ownerService.GetAllOwnersAsync()).ToList();

        if (!owners.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No records in database.");
            Console.ResetColor();
            Pause();
            return;
        }

        foreach (var owner in owners)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[OWNER] {owner.Name} - {owner.DocumentType}: {owner.DocumentNumber} (UUID: {owner.Uuid})");
            Console.WriteLine($"   Phone: {owner.Phone} | Email: {owner.Email} | Address: {owner.Address}");
            Console.ResetColor();

            if (!owner.Pets.Any())
            {
                Console.WriteLine("   |- (No registered pets)");
            }
            else
            {
                foreach (var pet in owner.Pets)
                {
                    Console.WriteLine($"   |- [PET] {pet.Name} (UUID: {pet.Uuid}) | Species: {pet.Species} | Breed: {pet.Breed} | Age: {pet.Age}y | Weight: {pet.Weight}kg | Symptoms: {pet.Symptoms}");
                }
            }
        }

        Pause();
    }
    #endregion

    private static string PromptDocumentType()
    {
        Console.WriteLine("Available Document Types:");
        Console.WriteLine("  1. CC (Citizenship Card)");
        Console.WriteLine("  2. CE (Foreigner Identity Card)");
        Console.WriteLine("  3. TI (Identity Card)");
        Console.WriteLine("  4. PASSPORT");
        Console.WriteLine("  5. NIT");
        Console.WriteLine("  6. PEP");
        Console.Write("Select Document Type (1-6) or type code: ");

        string input = Console.ReadLine()?.Trim().ToUpper() ?? "CC";
        return input switch
        {
            "1" => "CC",
            "2" => "CE",
            "3" => "TI",
            "4" => "PASSPORT",
            "5" => "NIT",
            "6" => "PEP",
            _ => OwnerService.ValidDocumentTypes.Contains(input) ? input : "CC"
        };
    }

    private static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[SUCCESS] {message}");
        Console.ResetColor();
    }

    private static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[ERROR] {message}");
        Console.ResetColor();
    }

    private static void Pause()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }
}
