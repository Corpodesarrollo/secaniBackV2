using Core.Interfaces.Repositorios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Repositorios
{
    public class AsignacionAutomaticaJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private IGeneralCOM _generalCOM;
        private Core.DTOs.Quartz _quartz;

        public AsignacionAutomaticaJob(IServiceProvider serviceProvider,IOptions<Core.DTOs.Quartz> optQuartz)
        {
            _serviceProvider = serviceProvider;
            _quartz = optQuartz.Value;
        }
        public Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _generalCOM = scope.ServiceProvider.GetRequiredService<IGeneralCOM>();
                _generalCOM.AsignarSeguimientoAutomatico(_quartz.URLBase + "/api/");
            }

            return Task.CompletedTask;
        }
    }
}
