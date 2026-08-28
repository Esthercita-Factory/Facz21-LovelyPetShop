using LovelyPetShop.CLI.Business.Services;
using LovelyPetShop.CLI.Domain.Entities;
using LovelyPetShop.CLI.Domain.Exceptions;
using LovelyPetShop.CLI.Domain.Interfaces;
using Xunit;

namespace LovelyPetShop.Tests;

public class AdvancedFeaturesTests
{
    private readonly FakeOwnerRepository _ownerRepo;
    private readonly FakePetRepository _petRepo;
    private readonly LinqReportService _linqReportService;
    private readonly LoggerService _loggerService;
    private readonly ClinicSimulationService _simulationService;

    public AdvancedFeaturesTests()
    {
        _ownerRepo = new FakeOwnerRepository();
        _petRepo = new FakePetRepository();
        _linqReportService = new LinqReportService(_petRepo, _ownerRepo);
        _loggerService = new LoggerService();
        _simulationService = new ClinicSimulationService(_loggerService);
    }

    [Fact]
    public void Polimorfismo_AnimalYEmitirSonido_RetornaSonidoSegunEspecie()
    {
        Animal perro = new Pet { Name = "Boby", Species = "Perro", Breed = "Labrador", Age = 4, Weight = 25 };
        Animal gato = new Pet { Name = "Michi", Species = "Gato", Breed = "Siamés", Age = 2, Weight = 4 };
        Animal loro = new Pet { Name = "Pepe", Species = "Loro", Breed = "Amazona", Age = 5, Weight = 0.5 };

        Assert.Equal("¡Guau guau!", perro.EmitirSonido());
        Assert.Equal("¡Miau miau!", gato.EmitirSonido());
        Assert.Equal("¡Pío pío / Kraaa!", loro.EmitirSonido());
    }

    [Fact]
    public void InterfacesMultiples_OwnerYPet_ImplementanIRegistrableEINotificable()
    {
        var owner = new Owner("CC", "123456", "Laura R.", "3100000000", "laura@mail.com", "Calle 1");
        var pet = new Pet("Luna", "Perro", "Beagle", 3, 10, "Control", "123456");

        Assert.IsAssignableFrom<IRegistrable>(owner);
        Assert.IsAssignableFrom<INotificable>(owner);
        Assert.IsAssignableFrom<IRegistrable>(pet);

        string resumenOwner = owner.ObtenerResumenRegistro();
        string resumenPet = pet.ObtenerResumenRegistro();

        Assert.Contains("Laura R.", resumenOwner);
        Assert.Contains("Luna", resumenPet);
    }

    [Fact]
    public void ClasesAbstractas_ServiciosVeterinarios_AtiendenCorrectamente()
    {
        var pet = new Pet("Toby", "Perro", "Golden", 5, 30, "Dolor estomacal", "123");

        ServicioVeterinario consulta = new ConsultaGeneral();
        ServicioVeterinario vacuna = new Vacunacion("Rabia");

        string resConsulta = consulta.Atender(pet);
        string resVacuna = vacuna.Atender(pet);

        Assert.Contains("CONSULTA", resConsulta);
        Assert.Contains("Toby", resConsulta);
        Assert.Contains("¡Guau guau!", resConsulta);

        Assert.Contains("VACUNACIÓN", resVacuna);
        Assert.Contains("Rabia", resVacuna);
    }

    [Fact]
    public async Task LinqReportService_AgruparYExtremosEdad_CalculaCorrectamente()
    {
        await _petRepo.AddAsync(new Pet("Alpha", "Perro", "Pastor", 8, 28, "Ninguno", "111"));
        await _petRepo.AddAsync(new Pet("Puppy", "Perro", "Pug", 1, 6, "Ninguno", "111"));
        await _petRepo.AddAsync(new Pet("Minino", "Gato", "Persa", 4, 4, "Ninguno", "222"));

        var agrupado = (await _linqReportService.AgruparMascotasPorEspecieAsync()).ToList();
        Assert.Equal(2, agrupado.Count);

        var perroGroup = agrupado.First(g => g.Especie == "Perro");
        Assert.Equal(2, perroGroup.Cantidad);
        Assert.Equal(4.5, perroGroup.PromedioEdad);

        var (joven, viejo) = await _linqReportService.ObtenerExtremosEdadMascotasAsync();
        Assert.NotNull(joven);
        Assert.NotNull(viejo);
        Assert.Equal("Puppy", joven.Name);
        Assert.Equal("Alpha", viejo.Name);
    }

    [Fact]
    public async Task LinqReportService_DiccionariosYQuerySyntax_PermiteAccesoRapido()
    {
        var pet = new Pet("Kira", "Perro", "Husky", 3, 20, "Fiebre", "333");
        await _petRepo.AddAsync(pet);

        var dict = await _linqReportService.ObtenerDiccionarioMascotasPorUuidAsync();
        Assert.True(dict.ContainsKey(pet.Uuid));
        Assert.Equal("Kira", dict[pet.Uuid].Name);

        var queryResults = (await _linqReportService.ConsultarConSintaxisQueryAsync("Perro", 2)).ToList();
        Assert.Single(queryResults);
        Assert.Equal("Kira", queryResults[0].Name);
    }

    [Fact]
    public async Task Concurrencia_TaskWhenAllYWhenAny_EjecutanSinBloqueos()
    {
        var pets = new List<Pet>
        {
            new("M1", "Perro", "Criollo", 2, 10, "", "1"),
            new("M2", "Gato", "Criollo", 3, 4, "", "2")
        };

        var resWhenAll = (await _simulationService.SimularProcesamientoParaleloWhenAllAsync(pets)).ToList();
        Assert.Equal(2, resWhenAll.Count);

        var resWhenAny = await _simulationService.SimularAsignacionRapidaWhenAnyAsync("Max");
        Assert.NotNull(resWhenAny);
        Assert.Contains("Max", resWhenAny.Detalle);
    }

    [Fact]
    public void ExcepcionesPersonalizadas_MascotaNoEncontrada_LanzaExceptionAdecuada()
    {
        Action act = () => throw new MascotaNoEncontradaException("Mascota con ID inexistente");
        Assert.Throws<MascotaNoEncontradaException>(act);
    }
}
