using LovelyPetShop.CLI.Business.Services;
using LovelyPetShop.CLI.Domain.Entities;
using LovelyPetShop.CLI.Domain.Exceptions;
using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.UI;

public class ConsoleMenu
{
    private readonly IOwnerService _ownerService;
    private readonly IPetService _petService;
    private readonly ILinqReportService _linqReportService;
    private readonly IClinicSimulationService _simulationService;
    private readonly ILoggerService _loggerService;
    private const int PageSize = 5;

    public ConsoleMenu(
        IOwnerService ownerService,
        IPetService petService,
        ILinqReportService linqReportService,
        IClinicSimulationService simulationService,
        ILoggerService loggerService)
    {
        _ownerService = ownerService;
        _petService = petService;
        _linqReportService = linqReportService;
        _simulationService = simulationService;
        _loggerService = loggerService;
    }

    public async Task RunAsync()
    {
        bool running = true;
        await _loggerService.LogInfoAsync("Inicio de sesión en la aplicación CLI LovelyPetShop.");

        while (running)
        {
            try
            {
                DisplayHeader();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("1. Gestión de Propietarios");
                Console.WriteLine("2. Gestión de Mascotas");
                Console.WriteLine("3. Registro Rápido (Mascota + Propietario en 1 paso)");
                Console.WriteLine("4. Reporte General (Propietarios y Mascotas)");
                Console.WriteLine("5. Consultas y Estadísticas Avanzadas (LINQ - HU2)");
                Console.WriteLine("6. Servicios Veterinarios y Polimorfismo (POO - HU3/HU4)");
                Console.WriteLine("7. Simulación Concurrente y Asíncrona (Async - HU5)");
                Console.WriteLine("8. Registro de Logs y Manejo de Excepciones (HU4)");
                Console.WriteLine("9. Salir");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nSeleccione una opción: ");
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
                        await LinqSubMenuAsync();
                        break;
                    case "6":
                        await VeterinaryServicesSubMenuAsync();
                        break;
                    case "7":
                        await ConcurrencySimulationSubMenuAsync();
                        break;
                    case "8":
                        await LogsSubMenuAsync();
                        break;
                    case "9":
                        running = false;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("¡Gracias por utilizar LovelyPetShop CLI! ¡Hasta pronto!");
                        Console.ResetColor();
                        await _loggerService.LogInfoAsync("Cierre normal de la sesión CLI.");
                        break;
                    default:
                        PrintError("Opción inválida. Intente de nuevo.");
                        Pause();
                        break;
                }
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync("Error inesperado en el bucle principal", ex);
                PrintError($"Se produjo un error no controlado: {ex.Message}");
                Pause();
            }
            finally
            {
                // Limpieza o acciones de fin de ciclo si fuesen necesarias
            }
        }
    }

    private static void DisplayHeader()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("+----------------------------------------------------------+");
        Console.WriteLine("|               LOVELY PET SHOP - VETERINARIA              |");
        Console.WriteLine("|            Gestión Integral de Mascotas (Colombia)       |");
        Console.WriteLine("+----------------------------------------------------------+");
        Console.ResetColor();
    }

    #region Menú de Propietarios (HU 1)
    private async Task OwnerSubMenuAsync()
    {
        bool inSubMenu = true;
        while (inSubMenu)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- GESTIÓN DE PROPIETARIOS ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Registrar nuevo propietario");
            Console.WriteLine("2. Buscar propietario por número de documento");
            Console.WriteLine("3. Listar todos los propietarios (Paginado)");
            Console.WriteLine("4. Actualizar datos de propietario");
            Console.WriteLine("5. Eliminar propietario");
            Console.WriteLine("6. Volver al menú principal");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nSeleccione opción: ");
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
                    PrintError("Opción inválida.");
                    Pause();
                    break;
            }
        }
    }

    private async Task CreateOwnerAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[+] REGISTRAR NUEVO PROPIETARIO");
        Console.ResetColor();

        string docType = PromptDocumentType();

        Console.Write("Número de Documento: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Nombre Completo: ");
        string name = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Teléfono de Contacto: ");
        string phone = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Correo Electrónico: ");
        string email = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Dirección de Residencia: ");
        string address = Console.ReadLine()?.Trim() ?? string.Empty;

        var result = await _ownerService.CreateOwnerAsync(docType, docNum, name, phone, email, address);
        if (result.Success)
        {
            PrintSuccess(result.Message);
            await _loggerService.LogInfoAsync($"Propietario creado: {name} ({docType} {docNum})");
        }
        else
        {
            PrintError(result.Message);
            await _loggerService.LogWarningAsync($"Fallo al crear propietario {name}: {result.Message}");
        }
    }

    private async Task ReadOwnerByDocumentAsync()
    {
        Console.Write("Número de Documento del Propietario: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var owner = await _ownerService.GetOwnerByDocumentAsync(docNum);
        if (owner == null)
        {
            PrintError($"No se encontró ningún propietario registrado con el documento '{docNum}'.");
            return;
        }

        DisplayOwnerDetails(owner);
    }

    private static void DisplayOwnerDetails(Owner owner)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[INFORMACIÓN] Detalle del Propietario");
        Console.WriteLine($"   UUID:                  {owner.Uuid}");
        Console.WriteLine($"   Tipo de Documento:     {owner.DocumentType}");
        Console.WriteLine($"   No. Documento:         {owner.DocumentNumber}");
        Console.WriteLine($"   Nombre Completo:       {owner.Name}");
        Console.WriteLine($"   Teléfono:              {owner.Phone}");
        Console.WriteLine($"   Email:                 {owner.Email}");
        Console.WriteLine($"   Dirección:             {owner.Address}");
        Console.WriteLine($"   Fecha de Registro:     {owner.CreatedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"   Mascotas Registradas:  {owner.Pets.Count}");

        foreach (var p in owner.Pets)
        {
            Console.WriteLine($"     - Mascota [{p.Name}] | Especie: {p.Species} | Raza: {p.Breed} | Edad: {p.Age} años | UUID: {p.Uuid}");
        }
        Console.ResetColor();
    }

    private async Task PaginateOwnersAsync()
    {
        var owners = (await _ownerService.GetAllOwnersAsync()).ToList();
        if (!owners.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No hay propietarios registrados en el sistema.");
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
            Console.WriteLine($"--- LISTADO DE PROPIETARIOS (Página {currentPage} de {totalPages} | Total: {owners.Count}) ---");
            Console.WriteLine("{0,-4} | {1,-10} | {2,-14} | {3,-20} | {4,-12} | {5,-36}", "#", "Tipo Doc", "No. Doc", "Nombre", "Teléfono", "UUID");
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
            Console.WriteLine("\n[N] Siguiente página | [P] Página anterior | [Q] Volver al menú");
            Console.Write("Seleccione opción: ");
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
        Console.Write("Ingrese el Número de Documento del propietario a actualizar: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var existing = await _ownerService.GetOwnerByDocumentAsync(docNum);
        if (existing == null)
        {
            PrintError($"No se encontró ningún propietario con el documento No. '{docNum}'.");
            return;
        }

        Console.WriteLine($"Actualizando datos de {existing.Name} (deje en blanco para conservar el valor actual):");

        Console.Write($"Nuevo Tipo Doc [{existing.DocumentType}] (Enter para conservar): ");
        string? newDocType = Console.ReadLine()?.Trim();
        newDocType = string.IsNullOrEmpty(newDocType) ? null : newDocType;

        Console.Write($"Nuevo No. Doc [{existing.DocumentNumber}]: ");
        string? newDocNum = Console.ReadLine()?.Trim();
        newDocNum = string.IsNullOrEmpty(newDocNum) ? null : newDocNum;

        Console.Write($"Nuevo Nombre [{existing.Name}]: ");
        string? name = Console.ReadLine()?.Trim();
        name = string.IsNullOrEmpty(name) ? null : name;

        Console.Write($"Nuevo Teléfono [{existing.Phone}]: ");
        string? phone = Console.ReadLine()?.Trim();
        phone = string.IsNullOrEmpty(phone) ? null : phone;

        Console.Write($"Nuevo Email [{existing.Email}]: ");
        string? email = Console.ReadLine()?.Trim();
        email = string.IsNullOrEmpty(email) ? null : email;

        Console.Write($"Nueva Dirección [{existing.Address}]: ");
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
        Console.Write("Ingrese el Número de Documento del propietario a eliminar: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var result = await _ownerService.DeleteOwnerAsync(docNum);
        if (result.Success)
            PrintSuccess(result.Message);
        else
            PrintError(result.Message);
    }
    #endregion

    #region Menú de Mascotas (HU 1)
    private async Task PetSubMenuAsync()
    {
        bool inSubMenu = true;
        while (inSubMenu)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- GESTIÓN DE MASCOTAS ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Registrar nueva mascota (para propietario existente)");
            Console.WriteLine("2. Registrar mascota y propietario (Registro 1 paso)");
            Console.WriteLine("3. Buscar mascota por UUID");
            Console.WriteLine("4. Listar mascotas por Documento del Propietario");
            Console.WriteLine("5. Listar todas las mascotas (Paginado)");
            Console.WriteLine("6. Actualizar datos de mascota");
            Console.WriteLine("7. Eliminar mascota");
            Console.WriteLine("8. Volver al menú principal");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nSeleccione opción: ");
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
                    PrintError("Opción inválida.");
                    Pause();
                    break;
            }
        }
    }

    private async Task CreatePetAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[+] REGISTRAR NUEVA MASCOTA");
        Console.ResetColor();

        Console.Write("Número de Documento del Propietario: ");
        string ownerDocNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var owner = await _ownerService.GetOwnerByDocumentAsync(ownerDocNum);
        if (owner == null)
        {
            PrintError($"No hay propietario registrado con el documento '{ownerDocNum}'.");
            return;
        }

        Console.WriteLine($"Propietario: {owner.Name} ({owner.DocumentType} {owner.DocumentNumber})");

        Console.Write("Nombre de la Mascota: ");
        string name = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Especie (ej. Perro, Gato, Ave): ");
        string species = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Raza (ej. Mestizo, Criollo, Poodle): ");
        string breed = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Edad (años): ");
        if (!int.TryParse(Console.ReadLine(), out int age))
        {
            PrintError("Edad no válida. Debe ingresar un número entero.");
            return;
        }

        Console.Write("Peso (kg): ");
        if (!double.TryParse(Console.ReadLine(), out double weight))
        {
            PrintError("Peso no válido. Debe ingresar un valor numérico.");
            return;
        }

        Console.Write("Síntomas / Motivo de consulta: ");
        string symptoms = Console.ReadLine()?.Trim() ?? string.Empty;

        var result = await _petService.CreatePetAsync(name, species, breed, age, weight, symptoms, ownerDocNum);
        if (result.Success)
        {
            PrintSuccess(result.Message);
            await _loggerService.LogInfoAsync($"Mascota creada: {name} ({species}) para dueño {ownerDocNum}");
        }
        else
        {
            PrintError(result.Message);
        }
    }

    private async Task CreatePetWithOwnerCombinedAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[+] REGISTRO CONJUNTO: MASCOTA + PROPIETARIO");
        Console.ResetColor();

        Console.WriteLine("\n--- DATOS DEL PROPIETARIO ---");
        string docType = PromptDocumentType();

        Console.Write("Número de Documento del Propietario: ");
        string docNum = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Nombre Completo del Propietario: ");
        string ownerName = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Teléfono de Contacto: ");
        string ownerPhone = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Correo Electrónico: ");
        string ownerEmail = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Dirección de Residencia: ");
        string ownerAddress = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.WriteLine("\n--- DATOS DE LA MASCOTA ---");
        Console.Write("Nombre de la Mascota: ");
        string petName = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Especie (ej. Perro, Gato): ");
        string species = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Raza (ej. Mestizo, Criollo): ");
        string breed = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Edad (años): ");
        if (!int.TryParse(Console.ReadLine(), out int age))
        {
            PrintError("Edad no válida.");
            return;
        }

        Console.Write("Peso (kg): ");
        if (!double.TryParse(Console.ReadLine(), out double weight))
        {
            PrintError("Peso no válido.");
            return;
        }

        Console.Write("Síntomas / Motivo de consulta: ");
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
        Console.Write("Ingrese el UUID de la Mascota: ");
        string uuid = Console.ReadLine()?.Trim() ?? string.Empty;

        var pet = await _petService.GetPetByUuidAsync(uuid);
        if (pet == null)
        {
            PrintError($"No se encontró ninguna mascota con el UUID '{uuid}'.");
            return;
        }

        var owner = await _ownerService.GetOwnerByDocumentAsync(pet.OwnerDocumentNumber);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[INFORMACIÓN] Detalle de la Mascota");
        Console.WriteLine($"   UUID:                  {pet.Uuid}");
        Console.WriteLine($"   Nombre:                {pet.Name}");
        Console.WriteLine($"   Especie:               {pet.Species}");
        Console.WriteLine($"   Raza:                  {pet.Breed}");
        Console.WriteLine($"   Edad:                  {pet.Age} años");
        Console.WriteLine($"   Peso:                  {pet.Weight} kg");
        Console.WriteLine($"   Sonido característico: {pet.EmitirSonido()}");
        Console.WriteLine($"   Síntomas:              {pet.Symptoms}");
        Console.WriteLine($"   Propietario:           {owner?.Name ?? "Desconocido"} (Doc No. {pet.OwnerDocumentNumber})");
        Console.WriteLine($"   Fecha de Registro:     {pet.CreatedAt:yyyy-MM-dd HH:mm}");
        Console.ResetColor();
    }

    private async Task ListPetsByOwnerDocumentAsync()
    {
        Console.Write("Ingrese el Número de Documento del Propietario: ");
        string ownerDocNum = Console.ReadLine()?.Trim() ?? string.Empty;

        var owner = await _ownerService.GetOwnerByDocumentAsync(ownerDocNum);
        if (owner == null)
        {
            PrintError($"No se encontró ningún propietario con el documento No. '{ownerDocNum}'.");
            return;
        }

        var pets = (await _petService.GetPetsByOwnerDocumentAsync(ownerDocNum)).ToList();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nMascotas asociadas a '{owner.Name}' ({owner.DocumentType} No. {owner.DocumentNumber}):");
        Console.ResetColor();

        if (!pets.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Este propietario no tiene mascotas registradas.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("{0,-4} | {1,-15} | {2,-10} | {3,-12} | {4,-6} | {5,-8} | {6,-36}", "#", "Nombre", "Especie", "Raza", "Edad", "Peso", "UUID Mascota");
        Console.WriteLine(new string('-', 100));
        int idx = 1;
        foreach (var p in pets)
        {
            Console.WriteLine("{0,-4} | {1,-15} | {2,-10} | {3,-12} | {4,-6} | {5,-8} | {6,-36}",
                idx++, Truncate(p.Name, 15), Truncate(p.Species, 10), Truncate(p.Breed, 12), $"{p.Age}a", $"{p.Weight}kg", p.Uuid);
        }
    }

    private async Task PaginatePetsAsync()
    {
        var pets = (await _petService.GetAllPetsAsync()).ToList();
        if (!pets.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No hay mascotas registradas en la clínica.");
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
            Console.WriteLine($"--- LISTADO DE MASCOTAS (Página {currentPage} de {totalPages} | Total: {pets.Count}) ---");
            Console.WriteLine("{0,-4} | {1,-15} | {2,-10} | {3,-12} | {4,-6} | {5,-8} | {6,-14} | {7,-36}", "#", "Nombre", "Especie", "Raza", "Edad", "Peso", "Doc Propietario", "UUID Mascota");
            Console.WriteLine(new string('-', 116));
            Console.ResetColor();

            var pagePets = pets.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();
            int itemIndex = (currentPage - 1) * PageSize + 1;

            foreach (var p in pagePets)
            {
                Console.WriteLine("{0,-4} | {1,-15} | {2,-10} | {3,-12} | {4,-6} | {5,-8} | {6,-14} | {7,-36}",
                    itemIndex++, Truncate(p.Name, 15), Truncate(p.Species, 10), Truncate(p.Breed, 12), $"{p.Age}a", $"{p.Weight}kg", Truncate(p.OwnerDocumentNumber, 14), p.Uuid);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[N] Siguiente página | [P] Página anterior | [Q] Volver al menú");
            Console.Write("Seleccione opción: ");
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
        Console.Write("Ingrese el UUID de la mascota a actualizar: ");
        string uuid = Console.ReadLine()?.Trim() ?? string.Empty;

        var existing = await _petService.GetPetByUuidAsync(uuid);
        if (existing == null)
        {
            PrintError($"No se encontró ninguna mascota con el UUID '{uuid}'.");
            return;
        }

        Console.WriteLine($"Actualizando datos de {existing.Name} (deje en blanco para conservar el valor actual):");

        Console.Write($"Nombre [{existing.Name}]: ");
        string? name = Console.ReadLine()?.Trim();
        name = string.IsNullOrEmpty(name) ? null : name;

        Console.Write($"Especie [{existing.Species}]: ");
        string? species = Console.ReadLine()?.Trim();
        species = string.IsNullOrEmpty(species) ? null : species;

        Console.Write($"Raza [{existing.Breed}]: ");
        string? breed = Console.ReadLine()?.Trim();
        breed = string.IsNullOrEmpty(breed) ? null : breed;

        Console.Write($"Edad [{existing.Age}]: ");
        string? inputAge = Console.ReadLine()?.Trim();
        int? age = string.IsNullOrEmpty(inputAge) ? null : int.TryParse(inputAge, out int a) ? a : null;

        Console.Write($"Peso [{existing.Weight} kg]: ");
        string? inputWeight = Console.ReadLine()?.Trim();
        double? weight = string.IsNullOrEmpty(inputWeight) ? null : double.TryParse(inputWeight, out double w) ? w : null;

        Console.Write($"Síntomas [{existing.Symptoms}]: ");
        string? symptoms = Console.ReadLine()?.Trim();
        symptoms = string.IsNullOrEmpty(symptoms) ? null : symptoms;

        Console.Write($"Nuevo Documento del Propietario [{existing.OwnerDocumentNumber}]: ");
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
        Console.Write("Ingrese el UUID de la mascota a eliminar: ");
        string uuid = Console.ReadLine()?.Trim() ?? string.Empty;

        var result = await _petService.DeletePetAsync(uuid);
        if (result.Success)
            PrintSuccess(result.Message);
        else
            PrintError(result.Message);
    }
    #endregion

    #region Menú LINQ Avanzado (HU 2)
    private async Task LinqSubMenuAsync()
    {
        bool inSubMenu = true;
        while (inSubMenu)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- CONSULTAS Y REPORTES LINQ (HISTORIA 2) ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Agrupar mascotas por especie (GroupBy + Conteo + Promedios)");
            Console.WriteLine("2. Encontrar extremos de edad (Mascota más joven y de mayor edad)");
            Console.WriteLine("3. Filtrar por especie y proyectar datos de contacto del dueño (Where + OrderBy + Select)");
            Console.WriteLine("4. Listar nombres en MAYÚSCULAS ordenados alfabéticamente (Select + OrderBy)");
            Console.WriteLine("5. Cuantificadores: Verificar condiciones con Any / All");
            Console.WriteLine("6. Búsqueda ultra-rápida indexada O(1) con Dictionary<string, T>");
            Console.WriteLine("7. Consulta con Sintaxis Query (Query Syntax)");
            Console.WriteLine("8. Volver al menú principal");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nSeleccione opción: ");
            Console.ResetColor();

            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await DisplayEspeciesGroupedAsync();
                    Pause();
                    break;
                case "2":
                    await DisplayExtremosEdadAsync();
                    Pause();
                    break;
                case "3":
                    await DisplayFilterAndProjectAsync();
                    Pause();
                    break;
                case "4":
                    await DisplayUppercaseNamesAsync();
                    Pause();
                    break;
                case "5":
                    await DisplayQuantifiersAsync();
                    Pause();
                    break;
                case "6":
                    await DisplayDictionaryLookupAsync();
                    Pause();
                    break;
                case "7":
                    await DisplayQuerySyntaxAsync();
                    Pause();
                    break;
                case "8":
                    inSubMenu = false;
                    break;
                default:
                    PrintError("Opción inválida.");
                    Pause();
                    break;
            }
        }
    }

    private async Task DisplayEspeciesGroupedAsync()
    {
        var grouped = (await _linqReportService.AgruparMascotasPorEspecieAsync()).ToList();
        if (!grouped.Any())
        {
            Console.WriteLine("No hay mascotas registradas para generar el reporte.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("{0,-18} | {1,-10} | {2,-18} | {3,-18}", "Especie", "Cantidad", "Promedio Edad (a)", "Promedio Peso (kg)");
        Console.WriteLine(new string('-', 72));
        Console.ResetColor();

        foreach (var item in grouped)
        {
            Console.WriteLine("{0,-18} | {1,-10} | {2,-18} | {3,-18}", item.Especie, item.Cantidad, $"{item.PromedioEdad} años", $"{item.PromedioPeso} kg");
        }
    }

    private async Task DisplayExtremosEdadAsync()
    {
        var (masJoven, masViejo) = await _linqReportService.ObtenerExtremosEdadMascotasAsync();
        if (masJoven == null || masViejo == null)
        {
            Console.WriteLine("No hay suficientes mascotas registradas.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[MASCOTA MÁS JOVEN] {masJoven.Name} ({masJoven.Species} - {masJoven.Breed}) | Edad: {masJoven.Age} año(s)");
        Console.WriteLine($"[MASCOTA MÁS LONGEVA] {masViejo.Name} ({masViejo.Species} - {masViejo.Breed}) | Edad: {masViejo.Age} año(s)");
        Console.ResetColor();
    }

    private async Task DisplayFilterAndProjectAsync()
    {
        Console.Write("Ingrese la especie a filtrar (ej. Perro, Gato): ");
        string especie = Console.ReadLine()?.Trim() ?? "Perro";

        var resultados = (await _linqReportService.FiltrarMascotasPorEspecieYOrdenarAsync(especie)).ToList();
        if (!resultados.Any())
        {
            Console.WriteLine($"No se encontraron mascotas de especie '{especie}'.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Resultados para especie '{especie}' ordenados por edad:");
        Console.WriteLine("{0,-15} | {1,-12} | {2,-6} | {3,-20} | {4,-12}", "Mascota", "Raza", "Edad", "Propietario", "Teléfono");
        Console.WriteLine(new string('-', 75));
        Console.ResetColor();

        foreach (var r in resultados)
        {
            Console.WriteLine("{0,-15} | {1,-12} | {2,-6} | {3,-20} | {4,-12}",
                Truncate(r.NombreMascota, 15), Truncate(r.Raza, 12), $"{r.Edad}a", Truncate(r.NombreDueno, 20), r.TelefonoDueno);
        }
    }

    private async Task DisplayUppercaseNamesAsync()
    {
        var nombres = await _linqReportService.ObtenerNombresMascotasEnMayusculasOrdenadosAsync();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Nombres de pacientes en MAYÚSCULAS (Orden alfabético):");
        Console.ResetColor();

        int i = 1;
        foreach (var nombre in nombres)
        {
            Console.WriteLine($" {i++}. {nombre}");
        }
    }

    private async Task DisplayQuantifiersAsync()
    {
        bool haySinRaza = await _linqReportService.ExisteMascotaConCondicionAsync(p =>
            string.IsNullOrWhiteSpace(p.Breed) || p.Breed.Contains("Criollo", StringComparison.OrdinalIgnoreCase) || p.Breed.Contains("Mestizo", StringComparison.OrdinalIgnoreCase));

        bool todasTienenSintomas = await _linqReportService.TodasLasMascotasCumplenCondicionAsync(p => !string.IsNullOrWhiteSpace(p.Symptoms));

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"• ¿Existe al menos una mascota Mestiza / Criolla? (LINQ .Any()): {(haySinRaza ? "SÍ" : "NO")}");
        Console.WriteLine($"• ¿Todas las mascotas registradas reportan síntomas? (LINQ .All()): {(todasTienenSintomas ? "SÍ" : "NO")}");
        Console.ResetColor();
    }

    private async Task DisplayDictionaryLookupAsync()
    {
        var dictMascotas = await _linqReportService.ObtenerDiccionarioMascotasPorUuidAsync();
        Console.WriteLine($"Diccionario en memoria indexado con {dictMascotas.Count} mascotas.");
        Console.Write("Ingrese UUID de la mascota a buscar en O(1): ");
        string uuid = Console.ReadLine()?.Trim() ?? string.Empty;

        if (dictMascotas.TryGetValue(uuid, out var pet))
        {
            PrintSuccess($"[ENCONTRADO EN DICCIONARIO] {pet.Name} ({pet.Species}) - Dueño Doc: {pet.OwnerDocumentNumber}");
        }
        else
        {
            PrintError($"No se encontró la clave '{uuid}' en el diccionario.");
        }
    }

    private async Task DisplayQuerySyntaxAsync()
    {
        Console.Write("Ingrese especie para consultar con Query Syntax (ej. Perro): ");
        string especie = Console.ReadLine()?.Trim() ?? "Perro";

        Console.Write("Edad mínima: ");
        int.TryParse(Console.ReadLine(), out int minAge);

        var resultados = (await _linqReportService.ConsultarConSintaxisQueryAsync(especie, minAge)).ToList();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[LINQ QUERY SYNTAX] Encontrados {resultados.Count} resultado(s):");
        Console.ResetColor();

        foreach (var p in resultados)
        {
            Console.WriteLine($" - {p.Name} ({p.Species}, {p.Age} años) - Raza: {p.Breed}");
        }
    }
    #endregion

    #region Menú Servicios Veterinarios y Polimorfismo (HU 3 y HU 4)
    private async Task VeterinaryServicesSubMenuAsync()
    {
        bool inSubMenu = true;
        while (inSubMenu)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- SERVICIOS VETERINARIOS Y POLIMORFISMO (HU 3 / HU 4) ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Atender Consulta Médica General (Clase Abstracta + Polimorfismo)");
            Console.WriteLine("2. Aplicar Plan de Vacunación (Clase Abstracta + Polimorfismo)");
            Console.WriteLine("3. Demostrar interfaces múltiples (IRegistrable e INotificable)");
            Console.WriteLine("4. Probar Polimorfismo de Sonidos Animales (EmitirSonido)");
            Console.WriteLine("5. Volver al menú principal");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nSeleccione opción: ");
            Console.ResetColor();

            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await RunConsultaVeterinariaAsync();
                    Pause();
                    break;
                case "2":
                    await RunVacunacionAsync();
                    Pause();
                    break;
                case "3":
                    await DemonstrateInterfacesAsync();
                    Pause();
                    break;
                case "4":
                    await TestAnimalSoundsAsync();
                    Pause();
                    break;
                case "5":
                    inSubMenu = false;
                    break;
                default:
                    PrintError("Opción inválida.");
                    Pause();
                    break;
            }
        }
    }

    private async Task RunConsultaVeterinariaAsync()
    {
        var pets = (await _petService.GetAllPetsAsync()).ToList();
        if (!pets.Any())
        {
            PrintError("No hay mascotas registradas para atender.");
            return;
        }

        var pet = pets.First();
        ServicioVeterinario servicio = new ConsultaGeneral("Revisión preventiva por pérdida de apetito");
        string resultado = servicio.Atender(pet);

        PrintSuccess(resultado);
        await _loggerService.LogInfoAsync($"Consulta ejecutada para mascota '{pet.Name}'");
    }

    private async Task RunVacunacionAsync()
    {
        var pets = (await _petService.GetAllPetsAsync()).ToList();
        if (!pets.Any())
        {
            PrintError("No hay mascotas registradas para vacunar.");
            return;
        }

        var pet = pets.Last();
        ServicioVeterinario servicio = new Vacunacion("Séxtuple Canina y Desparasitación");
        string resultado = servicio.Atender(pet);

        PrintSuccess(resultado);
        await _loggerService.LogInfoAsync($"Vacunación ejecutada para mascota '{pet.Name}'");
    }

    private async Task DemonstrateInterfacesAsync()
    {
        var owner = (await _ownerService.GetAllOwnersAsync()).FirstOrDefault();
        var pet = (await _petService.GetAllPetsAsync()).FirstOrDefault();

        if (owner == null || pet == null)
        {
            PrintError("Se requiere al menos un propietario y una mascota para la demostración.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("--- DEMOSTRACIÓN DE INTERFACES DE DOMINIO ---");
        Console.ResetColor();

        // IRegistrable en Owner y Pet
        IRegistrable regOwner = owner;
        IRegistrable regPet = pet;

        Console.WriteLine("[IRegistrable - Owner]: " + regOwner.ObtenerResumenRegistro());
        Console.WriteLine("[IRegistrable - Pet]:   " + regPet.ObtenerResumenRegistro());

        // INotificable en Owner
        INotificable notifOwner = owner;
        string notifRes = await notifOwner.EnviarNotificacionAsync("Recordatorio: Su mascota tiene programada cita médica mañana.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[INotificable]: " + notifRes);
        Console.ResetColor();
    }

    private async Task TestAnimalSoundsAsync()
    {
        var mascotas = (await _petService.GetAllPetsAsync()).ToList();
        if (!mascotas.Any())
        {
            PrintError("No hay mascotas registradas.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Polimorfismo en acción: Método Animal.EmitirSonido()");
        Console.ResetColor();

        foreach (var m in mascotas)
        {
            Animal animal = m; // Polimorfismo de asignación a clase base Animal
            Console.WriteLine($" • [{animal.Species}] {animal.Name} dice: {animal.EmitirSonido()}");
        }
    }
    #endregion

    #region Menú Concurrencia y Asincronía (HU 5)
    private async Task ConcurrencySimulationSubMenuAsync()
    {
        bool inSubMenu = true;
        while (inSubMenu)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- SIMULACIÓN CONCURRENTE Y ASÍNCRONA (HISTORIA 5) ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Procesamiento simultáneo de pacientes (Task.WhenAll)");
            Console.WriteLine("2. Asignación rápida de veterinario disponible (Task.WhenAny)");
            Console.WriteLine("3. Flujo completo de atención y notificación no bloqueante");
            Console.WriteLine("4. Volver al menú principal");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nSeleccione opción: ");
            Console.ResetColor();

            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await RunWhenAllSimulationAsync();
                    Pause();
                    break;
                case "2":
                    await RunWhenAnySimulationAsync();
                    Pause();
                    break;
                case "3":
                    await RunFullFlowSimulationAsync();
                    Pause();
                    break;
                case "4":
                    inSubMenu = false;
                    break;
                default:
                    PrintError("Opción inválida.");
                    Pause();
                    break;
            }
        }
    }

    private async Task RunWhenAllSimulationAsync()
    {
        var pets = await _petService.GetAllPetsAsync();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[Task.WhenAll] Lanzando tareas simultáneas en segundo plano...");
        Console.ResetColor();

        var resultados = await _simulationService.SimularProcesamientoParaleloWhenAllAsync(pets);

        foreach (var r in resultados)
        {
            Console.WriteLine($" -> {r.Operacion} ({r.DuracionMs} ms): {r.Detalle}");
        }
        PrintSuccess("Todas las tareas paralelas finalizaron exitosamente.");
    }

    private async Task RunWhenAnySimulationAsync()
    {
        Console.Write("Nombre de la mascota en urgencia: ");
        string petName = Console.ReadLine()?.Trim() ?? "Mascota";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[Task.WhenAny] Consultando 3 salas de veterinaria concurrentemente...");
        Console.ResetColor();

        var resultado = await _simulationService.SimularAsignacionRapidaWhenAnyAsync(petName);
        PrintSuccess(resultado.Detalle);
    }

    private async Task RunFullFlowSimulationAsync()
    {
        var owners = (await _ownerService.GetAllOwnersAsync()).ToList();
        var pets = (await _petService.GetAllPetsAsync()).ToList();

        if (!owners.Any() || !pets.Any())
        {
            PrintError("Se requiere al menos un propietario y una mascota.");
            return;
        }

        var pet = pets.First();
        var owner = owners.FirstOrDefault(o => o.DocumentNumber == pet.OwnerDocumentNumber) ?? owners.First();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Ejecutando flujo asíncrono completo para '{pet.Name}' de '{owner.Name}'...");
        Console.ResetColor();

        string resumen = await _simulationService.SimularFlujoCompletoAtencionAsync(pet, owner);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(resumen);
        Console.ResetColor();
    }
    #endregion

    #region Menú Logs y Excepciones (HU 4)
    private async Task LogsSubMenuAsync()
    {
        bool inSubMenu = true;
        while (inSubMenu)
        {
            DisplayHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- LOGS Y MANEJO ESTRUCTURADO DE EXCEPCIONES (HISTORIA 4) ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1. Ver últimos registros del log (clinic_events.log)");
            Console.WriteLine("2. Registrar evento informativo manual");
            Console.WriteLine("3. Simular y capturar excepción personalizada (MascotaNoEncontradaException)");
            Console.WriteLine("4. Volver al menú principal");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\nSeleccione opción: ");
            Console.ResetColor();

            string? choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await DisplayRecentLogsAsync();
                    Pause();
                    break;
                case "2":
                    await AddManualLogAsync();
                    Pause();
                    break;
                case "3":
                    await SimulateCustomExceptionAsync();
                    Pause();
                    break;
                case "4":
                    inSubMenu = false;
                    break;
                default:
                    PrintError("Opción inválida.");
                    Pause();
                    break;
            }
        }
    }

    private async Task DisplayRecentLogsAsync()
    {
        var logs = (await _loggerService.GetRecentLogsAsync(15)).ToList();
        if (!logs.Any())
        {
            Console.WriteLine("No hay registros de log disponibles todavía.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("--- ÚLTIMOS EVENTOS DEL SISTEMA ---");
        Console.ResetColor();

        foreach (var log in logs)
        {
            Console.WriteLine(log);
        }
    }

    private async Task AddManualLogAsync()
    {
        Console.Write("Mensaje a registrar en el log: ");
        string mensaje = Console.ReadLine()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(mensaje)) return;

        await _loggerService.LogInfoAsync($"[MANUAL] {mensaje}");
        PrintSuccess("Evento registrado exitosamente en el archivo de log.");
    }

    private async Task SimulateCustomExceptionAsync()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Simulando búsqueda forzada de UUID inexistente para probar try-catch-finally con MascotaNoEncontradaException...");
        Console.ResetColor();

        try
        {
            string uuidFalso = "00000000-0000-0000-0000-000000000000";
            var pet = await _petService.GetPetByUuidAsync(uuidFalso);
            if (pet == null)
            {
                throw new MascotaNoEncontradaException($"La mascota con UUID '{uuidFalso}' no existe en la base de datos.");
            }
        }
        catch (MascotaNoEncontradaException mex)
        {
            await _loggerService.LogErrorAsync("Capturada excepción personalizada", mex);
            PrintError($"[EXCEPCIÓN PERSONALIZADA CAPTURADA]: {mex.Message}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("El programa manejó el error con elegancia y continuó su ejecución sin romperse.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            await _loggerService.LogErrorAsync("Excepción general capturada", ex);
            PrintError($"[EXCEPCIÓN GENERAL]: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("[FINALLY] Bloque finally ejecutado: recursos y conexiones liberados.");
        }
    }
    #endregion

    #region Reporte General (HU 1)
    private async Task DisplayFullReportAsync()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==========================================================");
        Console.WriteLine("       REPORTE GENERAL: PROPIETARIOS Y MASCOTAS           ");
        Console.WriteLine("==========================================================");
        Console.ResetColor();

        var owners = (await _ownerService.GetAllOwnersAsync()).ToList();

        if (!owners.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No hay registros en la base de datos.");
            Console.ResetColor();
            Pause();
            return;
        }

        foreach (var owner in owners)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[PROPIETARIO] {owner.Name} - {owner.DocumentType}: {owner.DocumentNumber} (UUID: {owner.Uuid})");
            Console.WriteLine($"   Teléfono: {owner.Phone} | Email: {owner.Email} | Dirección: {owner.Address}");
            Console.ResetColor();

            if (!owner.Pets.Any())
            {
                Console.WriteLine("   |- (Sin mascotas registradas)");
            }
            else
            {
                foreach (var pet in owner.Pets)
                {
                    Console.WriteLine($"   |- [MASCOTA] {pet.Name} (UUID: {pet.Uuid}) | Especie: {pet.Species} | Raza: {pet.Breed} | Edad: {pet.Age}a | Peso: {pet.Weight}kg | Sonido: '{pet.EmitirSonido()}' | Síntomas: {pet.Symptoms}");
                }
            }
        }

        Pause();
    }
    #endregion

    private static string PromptDocumentType()
    {
        Console.WriteLine("Tipos de documento válidos en Colombia:");
        Console.WriteLine("  1. CC  - Cédula de Ciudadanía");
        Console.WriteLine("  2. CE  - Cédula de Extranjería");
        Console.WriteLine("  3. TI  - Tarjeta de Identidad");
        Console.WriteLine("  4. RC  - Registro Civil");
        Console.WriteLine("  5. NIT - Número de Identificación Tributaria");
        Console.WriteLine("  6. PASAPORTE - Pasaporte");
        Console.WriteLine("  7. PEP - Permiso Especial de Permanencia");
        Console.WriteLine("  8. PPT - Permiso por Protección Temporal");
        Console.Write("Seleccione el Tipo de Documento (1-8) o escriba el código: ");

        string input = Console.ReadLine()?.Trim().ToUpper() ?? "CC";
        return input switch
        {
            "1" => "CC",
            "2" => "CE",
            "3" => "TI",
            "4" => "RC",
            "5" => "NIT",
            "6" => "PASAPORTE",
            "7" => "PEP",
            "8" => "PPT",
            _ => OwnerService.ValidDocumentTypes.Contains(input) ? input : "CC"
        };
    }

    private static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[ÉXITO] {message}");
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
        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }
}
