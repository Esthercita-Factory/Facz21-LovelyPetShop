using LovelyPetShop.CLI.Business.Services;
using LovelyPetShop.CLI.DataAccess.Repositories;
using LovelyPetShop.CLI.UI;

// Inicialización de la capa de acceso a datos (JSON Repositories)
var ownerRepository = new JsonOwnerRepository();
var petRepository = new JsonPetRepository();

// Inicialización de servicios de negocio
var loggerService = new LoggerService();
var ownerService = new OwnerService(ownerRepository, petRepository);
var petService = new PetService(petRepository, ownerRepository, ownerService);
var linqReportService = new LinqReportService(petRepository, ownerRepository);
var clinicSimulationService = new ClinicSimulationService(loggerService);

// Inicialización y ejecución del menú principal por consola
var consoleMenu = new ConsoleMenu(
    ownerService,
    petService,
    linqReportService,
    clinicSimulationService,
    loggerService);

await consoleMenu.RunAsync();