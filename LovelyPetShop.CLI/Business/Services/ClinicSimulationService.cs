using System.Diagnostics;
using LovelyPetShop.CLI.Domain.Entities;
using LovelyPetShop.CLI.Domain.Interfaces;

namespace LovelyPetShop.CLI.Business.Services;

public class ClinicSimulationService : IClinicSimulationService
{
    private readonly ILoggerService _logger;

    public ClinicSimulationService(ILoggerService logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simula el procesamiento paralelo y concurrente de múltiples mascotas usando Task.WhenAll.
    /// Ejecuta simultáneamente el triaje, validación y preparación de historias clínicas sin bloquear el hilo principal.
    /// </summary>
    public async Task<IEnumerable<SimulacionResultado>> SimularProcesamientoParaleloWhenAllAsync(IEnumerable<Pet> mascotas)
    {
        var listaMascotas = mascotas.Take(5).ToList();
        if (!listaMascotas.Any())
        {
            return new List<SimulacionResultado>
            {
                new("Procesamiento Paralelo", "No hay mascotas para procesar", 0)
            };
        }

        await _logger.LogInfoAsync($"Iniciando simulación concurrente con Task.WhenAll para {listaMascotas.Count} mascotas.");

        var stopwatch = Stopwatch.StartNew();

        // Creamos un arreglo de tareas asíncronas concurrentes
        var tareas = listaMascotas.Select(async (pet, index) =>
        {
            var swIndividual = Stopwatch.StartNew();
            // Simulación de latencia variable (ej. I/O o cálculos de laboratorio)
            int delayMs = 100 + (index * 80);
            await Task.Delay(delayMs);
            swIndividual.Stop();

            string detalle = $"[Procesado en paralelo] Mascota '{pet.Name}' ({pet.Species}) | Sonido: {pet.EmitirSonido()} | Triaje OK.";
            return new SimulacionResultado($"Atención Concurrente #{index + 1}", detalle, swIndividual.ElapsedMilliseconds);
        });

        // Esperamos a que TODAS las tareas finalicen concurrentemente sin bloquear
        var resultados = await Task.WhenAll(tareas);

        stopwatch.Stop();
        await _logger.LogInfoAsync($"Finalizada simulación con Task.WhenAll en {stopwatch.ElapsedMilliseconds} ms.");

        return resultados;
    }

    /// <summary>
    /// Simula la competencia entre múltiples veterinarios / salas de urgencia usando Task.WhenAny.
    /// La primera sala o médico que responda atiende a la mascota.
    /// </summary>
    public async Task<SimulacionResultado> SimularAsignacionRapidaWhenAnyAsync(string nombreMascota)
    {
        await _logger.LogInfoAsync($"Buscando disponibilidad rápida de veterinarios para '{nombreMascota}' usando Task.WhenAny.");

        var random = new Random();
        var sw = Stopwatch.StartNew();

        // Creamos 3 tareas que simulan la respuesta de 3 veterinarios en salas distintas
        var vet1 = Task.Run(async () =>
        {
            int delay = random.Next(150, 350);
            await Task.Delay(delay);
            return ($"Dra. Valentina (Sala Cirugía 1)", delay);
        });

        var vet2 = Task.Run(async () =>
        {
            int delay = random.Next(100, 300);
            await Task.Delay(delay);
            return ($"Dr. Mateo (Consultorio General 2)", delay);
        });

        var vet3 = Task.Run(async () =>
        {
            int delay = random.Next(120, 280);
            await Task.Delay(delay);
            return ($"Dr. Santiago (Sala de Urgencias)", delay);
        });

        var listaTareas = new List<Task<(string Medico, int Delay)>> { vet1, vet2, vet3 };

        // Esperamos a que la PRIMERA tarea en responder esté lista
        var primeraTareaFinalizada = await Task.WhenAny(listaTareas);
        var (medicoGanador, tiempoRespuesta) = await primeraTareaFinalizada;

        sw.Stop();

        string detalle = $"La mascota '{nombreMascota}' fue asignada con éxito al primer disponible: {medicoGanador} en {tiempoRespuesta} ms.";
        await _logger.LogInfoAsync(detalle);

        return new SimulacionResultado("Asignación Rápida (WhenAny)", detalle, sw.ElapsedMilliseconds);
    }

    /// <summary>
    /// Simula el flujo completo de atención clínica combinando servicios, polimorfismo y notificaciones asíncronas.
    /// </summary>
    public async Task<string> SimularFlujoCompletoAtencionAsync(Pet mascota, Owner propietario)
    {
        var consulta = new ConsultaGeneral("Revisión de rutina preventiva");
        var vacuna = new Vacunacion("Rabia y Parvovirus");

        string resConsulta = consulta.Atender(mascota);
        await Task.Delay(100);

        string resVacuna = vacuna.Atender(mascota);
        await Task.Delay(100);

        string notif = await propietario.EnviarNotificacionAsync(
            $"Estimado/a {propietario.Name}, la atención de '{mascota.Name}' ha finalizado exitosamente.");

        await _logger.LogInfoAsync($"Flujo de atención completado para mascota '{mascota.Name}' del propietario '{propietario.Name}'.");

        return $"{resConsulta}\n{resVacuna}\n{notif}";
    }
}
