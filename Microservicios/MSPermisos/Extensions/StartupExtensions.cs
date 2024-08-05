using Core.Interfaces.Repositorios.MSPermisos;
using Core.Services.MSPermisos;
using Core.Validators.MSPermisos;
using FluentValidation;
using Infra;
using Infra.Repositorios.MSPermisos;
using Microsoft.EntityFrameworkCore;

namespace MSPermisos.Application
{
    public static class StartupExtensions
    {
        public static WebApplicationBuilder CustomConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IFuncionalidadService, FuncionalidadService>();
            builder.Services.AddTransient<IModuloService, ModuloService>();
            builder.Services.AddTransient<IPermisoService, PermisoService>();

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddTransient<IFuncionalidadRepository, FuncionalidadRepository>();
            builder.Services.AddTransient<IModuloRepository, ModuloRepository>();
            builder.Services.AddTransient<IPermisoRepository, PermisoRepository>();

            builder.Services.AddMemoryCache();

            builder.Services.AddValidatorsFromAssemblyContaining<PermisoValidator>();

            return builder;
        }
    }
}
