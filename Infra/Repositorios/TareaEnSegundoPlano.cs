using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infra.Repositorios
{
    public class TareaEnSegundoPlano(IServiceProvider serviceProvider, IConfiguration configuration, IIpsRepo ipsRepo) : IJob
    {

        //private async void EjecutarTarea(object state)
        //{
        //    using var scopes = serviceProvider.CreateScope();
        //    using var db = scopes.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //    db.Log.Add(new() { Mensaje = "Ejecutando tarea en segundo plano" });
        //    db.SaveChanges();

        //    if (_isRunning)
        //    {
        //        return; // Si la tarea ya se está ejecutando, no la ejecuta de nuevo
        //    }

        //    _isRunning = true;

        //    try
        //    {
        //        // Tu lógica de la tarea aquí
        //        await RealizarTareaAsincrona();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error en la ejecución de la tarea: {ex.Message}");
        //    }
        //    finally
        //    {
        //        _isRunning = false; // Marca la tarea como no en ejecución
        //    }
        //}

        //private async Task RealizarTareaAsincrona()
        //{
        //    using var scopes = serviceProvider.CreateScope();
        //    using var db = scopes.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //    db.Log.Add(new() { Mensaje = "Realizando tarea asincrónica" });
        //    db.SaveChanges();

        //    using var scope = serviceProvider.CreateScope();
        //    var ipsRepo = scope.ServiceProvider.GetRequiredService<IIpsRepo>();
        //    await ipsRepo.LoadData();
        //}

        //public override void Dispose()
        //{
        //    _timer?.Dispose();
        //    base.Dispose();
        //}

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using var scopes = serviceProvider.CreateScope();
                using var db = scopes.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Log.Add(new() { Mensaje = "Realizando tarea asincrónica" });
                db.SaveChanges();

                // Ejecutar la lógica del método asíncrono
                await ipsRepo.LoadData();

            }
            catch (Exception ex)
            {
                using var scopes = serviceProvider.CreateScope();
                using var db = scopes.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Log.Add(new() { Mensaje = $"Error en la ejecución de la tarea: {ex.Message}" });
                db.SaveChanges();
            }
        }
    }
}
