using Core.Common;
using Core.Interfaces;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Interfaces.Repositorios.Llamadas;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Llamadas;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Interfaces.Services.Reportes;
using Core.Modelos;
using Core.Modelos.Identity;
using Core.Modelos.TablasParametricas;
using Core.Services.Llamadas;
using Core.Services.MSTablasParametricas;
using Core.Services.MSUsuariosyRoles;
using Core.Services.Reportes;
using Core.Services.StorageService;
using Core.Validators.MSPermisos;
using Infra;
using Infra.Repositories;
using Infra.Repositories.Common;
using Infra.Repositorios;
using Infra.Repositorios.Llamadas;
using Infra.Repositorios.Reportes;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using MSSeguimiento.Api.Extensions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SISPRO.TRV.General;
using SISPRO.TRV.Web.MVCCore.Helpers;
using SISPRO.TRV.Web.MVCCore.StartupExtensions;
using System.Text.Json;

WebApplicationBuilder builder = WebApplicationHelper.CreateCustomBuilder<Program>(args);

ReadConfig.FixLoadAppSettings(builder.Configuration);

// Registro de los servicios
builder.CustomConfigureServices();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCustomConfigureServicesPreviousMvc();
builder
    .Services
    .AddCustomMvcControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new ByteArrayConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddCustomSwagger();

builder.Services.AddCustomAuthentication(true);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddScoped(typeof(GenericRepository<NNAs>));
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
builder.Services.AddScoped<IReportesSIVIGILARepo, ReportesSIVIGILARepo>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IGenericRepository<TPCIE10>, GenericRepository<TPCIE10>>();
builder.Services.AddScoped<INotificacionRepo, NotificacionRepo>();
builder.Services.AddScoped<IAlertaRepo, AlertaRepo>();
builder.Services.AddScoped<ISeguimientoRepo, SeguimientoRepo>();
builder.Services.AddScoped<IIntentoRepo, IntentoRepo>();
builder.Services.AddScoped<IDashboardRepo, DashboardRepo>();
builder.Services.AddScoped<INotificacionRepo, NotificacionRepo>();
builder.Services.AddScoped<IAdjuntosRepo, AdjuntosRepo>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IReporteDepuracionRepository, ReporteDepuracionRepository>();
builder.Services.AddScoped<IReporteDepuracionService, ReporteDepuracionService>();
builder.Services.AddScoped<IReporteDinamicoNNARepository, ReporteDinamicoNNARepository>();
builder.Services.AddScoped<IReporteDinamicoNNAService, ReporteDinamicoNNAService>();
builder.Services.AddScoped<IReporteDinamicoSeguimientoRepository, ReporteDinamicoSeguimientoRepository>();
builder.Services.AddScoped<IReporteDinamicoSeguimientoService, ReporteDinamicoSeguimientoService>();
builder.Services.AddScoped<IReporteDetalleRegDepuradosRepository, ReporteDetalleRegDepuradosRepository>();
builder.Services.AddScoped<IReporteDetalleRegDepuradosService, ReporteDetalleRegDepuradosService>();
builder.Services.AddScoped<TablaParametricaService>();
builder.Services.AddScoped<IEnviarRespuesta, EnviarRespuestaRepo>();
builder.Services.AddScoped<IGestionarAlertas, GestionarAlertasRepo>();
builder.Services.AddScoped<IReporteDinamicoAlertasRepository, ReporteDinamicoAlertasRepository>();
builder.Services.AddScoped<IReporteDinamicoAlertasService, ReporteDinamicoAlertasService>();
builder.Services.AddScoped<IResumenLlamadasRepository, ResumenLlamadasRepository>();
builder.Services.AddScoped<IResumenLlamadasService, ResumenLlamadasService>();
builder.Services.AddHttpClient();

// Register Quartz services
builder.Services.AddSingleton<IJobFactory, SingletonJobFactory>();
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
builder.Services.AddSingleton<IJob, AsignacionAutomaticaJob>();

var temporizadorAsignacionAutomatica = builder.Configuration.GetValue<string>("Quartz:AsignacionAutomaticaSeguimientos");

builder.Services.AddHostedService<QuartzHostedService>();

builder.Services.Configure<Core.DTOs.Quartz>(builder.Configuration.GetSection("Quartz"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins(
            "http://192.168.152.17:8140",
            "https://secani.sispropreprod.gov.co",
            "http://192.168.110.11:8140",
            "http://localhost:4200",
            "https://localhost:4200",
            "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
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