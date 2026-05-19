using Core.Interfaces;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Interfaces.Repositorios.Llamadas;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services;
using Core.Interfaces.Services.Llamadas;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Interfaces.Services.Reportes;
using Core.Modelos;
using Core.Services;
using Core.Services.Llamadas;
using Core.Services.MSTablasParametricas;
using Core.Services.Reportes;
using Core.Services.StorageService;
using Infra.Middleware;
using Infra.Repositories;
using Infra.Repositories.Common;
using Infra.Repositorios;
using Infra.Repositorios.Llamadas;
using Infra.Repositorios.Reportes;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MSNNA.Api.Extensions;
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

builder.Services.AddCustomSwagger();

builder.Services.AddCustomAuthentication(true);

// Registro de los servicios
builder.CustomConfigureServices();

//Registro de Repos
builder.Services.AddScoped(typeof(GenericRepository<NNAs>));
builder.Services.AddScoped(typeof(GenericRepository<>));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));

builder.Services.AddScoped<IContactoNNARepo, ContactoNNARepo>();
builder.Services.AddScoped<TablaParametricaService>();
builder.Services.AddScoped<INotificacionRepo, NotificacionRepo>();
builder.Services.AddScoped<INNARepo, NNARepo>();
builder.Services.AddScoped<INNAService, NNAService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IReportesSIVIGILARepo, ReportesSIVIGILARepo>();
builder.Services.AddScoped<IHelperRepo, HelperRepo>();
builder.Services.AddScoped<IAdjuntosRepo, AdjuntosRepo>();
builder.Services.AddTransient<ICuidadorRepo, CuidadorRepo>();
builder.Services.AddTransient<IPersonaService, PersonaService>();
builder.Services.AddTransient<IReporteInconsistenciaPersonaRepository, ReporteInconsistenciaPersonaRepository>();
builder.Services.AddTransient<IReporteInconsistenciaPersonaService, ReporteInconsistenciaPersonaService>();
builder.Services.AddTransient<Client>();
builder.Services.AddTransient<IReporteDinamicoNNAService, ReporteDinamicoNNAService>();
builder.Services.AddTransient<IReporteDinamicoNNARepository, ReporteDinamicoNNARepository>();
builder.Services.AddTransient<ISeguimientoRepo, SeguimientoRepo>();
builder.Services.AddScoped<IReporteDinamicoEAPBRepository, ReporteDinamicoEAPBRepository>();
builder.Services.AddScoped<IReporteDinamicoEAPBService, ReporteDinamicoEAPBService>();
builder.Services.AddScoped<IResumenLlamadasService, ResumenLlamadasService>();
builder.Services.AddScoped<IResumenLlamadasRepository, ResumenLlamadasRepository>();
builder.Services.AddScoped<INotificacionRepo, NotificacionRepo>();
builder.Services.AddScoped<IIpsRepo, IpsRepo>();
builder.Services.AddScoped<IEAPBRepo, EAPBRepo>();
builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
builder.Services.AddScoped<INNARepo, NNARepo>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins(
            "https://secani.sispro.gov.co",
            "http://192.168.152.17:8140",
            "https://secani.sispropreprod.gov.co",
            "http://192.168.110.11:8140",
            "http://localhost:4200",
            "https://localhost:4200",
            "http://localhost:9110",
            "https://localhost:9110",
            "http://18.232.27.199:9110",
            "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

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