using Core.Interfaces;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Services;
using Core.Services.MSTablasParametricas;
using Core.Services.StorageService;
using Core.Validators;
using Core.Validators.MSPermisos;
using FluentValidation;
using Infra;
using Infra.Repositories;
using Infra.Repositories.Common;
using Infra.Repositories.MSTablasParametricas;
using Infra.Repositorios;
using Infra.Repositorios.MSTablasParametricas;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Quartz;
using SISPRO.TRV.General;
using SISPRO.TRV.Web.MVCCore.Helpers;
using SISPRO.TRV.Web.MVCCore.StartupExtensions;
using System.Text.Json;

WebApplicationBuilder builder = WebApplicationHelper.CreateCustomBuilder<Program>(args);

ReadConfig.FixLoadAppSettings(builder.Configuration);

builder.Services.AddCustomConfigureServicesPreviousMvc();
builder
    .Services
    .AddCustomMvcControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddControllersWithViews();

////builder.Services.AddHostedService<TareaEnSegundoPlano>();
//builder.Services.AddHostedService<ProcesadorTareasAutomaticas>();
//builder.Services.AddScoped<IProcesoAutomatico, ProcesoEnviarCorreos>();

builder.Services.AddCustomSwagger();

builder.Services.AddCustomAuthentication(true);

builder.Services.AddScoped(typeof(GenericRepository<>));

builder.Services.AddScoped<IAdjuntosRepo, AdjuntosRepo>();
builder.Services.AddScoped<IStorageService, StorageService>();

builder.Services.AddScoped<ICategoriaAlertaRepository, CategoriaAlertaRepository>();
builder.Services.AddScoped<IHistoricoTransaccionRepository, HistoricoTransaccionRepository>();
builder.Services.AddScoped<ICategoriaAlertaService, CategoriaAlertaService>();
builder.Services.AddScoped<INombreTablaParametricaService, NombresTablaParametricaService>();
builder.Services.AddScoped<ITablaParametricaRepository, TablaParametricaRepository>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));

builder.Services.AddScoped<IContactoEntidadRepository, ContactoEntidadRepository>();
builder.Services.AddScoped<IContactoEntidadService, ContactoEntidadService>();

builder.Services.AddValidatorsFromAssemblyContaining<ContactoEntidadRequestValidator>();
builder.Services.AddScoped<IFestivoService, FestivoService>();
builder.Services.AddScoped<IHistoricoTransaccionService, HistoricoTransaccionService>();
builder.Services.AddScoped<IFestivosRepository, FestivosRepository>();
builder.Services.AddScoped<ITPParentescos, TPParentescosRepo>();
builder.Services.AddScoped<IIpsRepo, IpsRepo>();
builder.Services.AddScoped<ISeguimientoRepo, SeguimientoRepo>();
builder.Services.AddScoped<ReportesSIVIGILARepo, ReportesSIVIGILARepo>();
builder.Services.AddScoped<ReportesSIVIGILARepo, ReportesSIVIGILARepo>();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

builder.Services.AddHttpClient<TablaParametricaService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://192.168.110.11:8140", "http://localhost:4200", "https://localhost:4200", "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>()
                .AddCheck<CustomHealthCheck>("CustomHealthCheck");

WebApplication app = builder.Build();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = "El servicio esta disponible"
        });
        await context.Response.WriteAsync(result);
    }
});

app.UseCors("AllowSpecificOrigin");
app.UseCustomConfigure();
app.UseCustomSwagger();

app.Run();
