using Core.CQRS.MSUsuariosyRoles.Queries.User;
using Core.Interfaces;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Interfaces.Repositorios.Llamadas;
using Core.Interfaces.Repositorios.MSPermisos;
using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Base;
using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Query.Base;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Llamadas;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Interfaces.Services.Reportes;
using Core.Modelos;
using Core.Modelos.Identity;
using Core.Modelos.TablasParametricas;
using Core.Services.Llamadas;
using Core.Services.MSPermisos;
using Core.Services.MSTablasParametricas;
using Core.Services.MSUsuariosyRoles;
using Core.Services.Reportes;
using Core.Services.StorageService;
using DinkToPdf;
using DinkToPdf.Contracts;
using Infra;
using Infra.Repositories;
using Infra.Repositories.Common;
using Infra.Repositorios;
using Infra.Repositorios.Llamadas;
using Infra.Repositorios.MSPermisos;
using Infra.Repositorios.MSUsuariosyRoles.Command.Base;
using Infra.Repositorios.MSUsuariosyRoles.Query.Base;
using Infra.Repositorios.Reportes;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSEntidad.Api.Extensions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SISPRO.TRV.Entity.Helpers;
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

// Asegurar que CORS maneje OPTIONS autom�ticamente
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins(
            "https://secani.sispro.gov.co",
            "http://192.168.152.17:8140",
            "https://secani.sispropreprod.gov.co",
            "https://nna.sispropreprod.gov.co", // Agregando el dominio de la API tambi�n
            "http://192.168.110.11:8140",
            "http://localhost:4200",
            "https://localhost:4200",
            "http://localhost:9110",
            "https://localhost:9110",
            "http://18.232.27.199:9110",
            "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
                          .AllowAnyMethod()  // Esto incluye OPTIONS autom�ticamente
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromMinutes(30))); // Cache preflight por 30 min
});

builder.Services.AddCustomSwagger();

builder.Services.AddCustomAuthentication(true);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Core.Authorization.PoliticasPermisos.RequiereCoordinadorAdmin, policy =>
        policy.Requirements.Add(new Core.Authorization.RequiereRolRequirement(
            Core.Authorization.PoliticasPermisos.RolesSispro.CoordinadorAdmin)));

    options.AddPolicy(Core.Authorization.PoliticasPermisos.RequiereAgenteOAdmin, policy =>
        policy.Requirements.Add(new Core.Authorization.RequiereRolRequirement(
            Core.Authorization.PoliticasPermisos.RolesSispro.CoordinadorAdmin,
            Core.Authorization.PoliticasPermisos.RolesSispro.AgenteSeguimiento)));
});
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Core.Authorization.RequiereRolHandler>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetUserQuery).Assembly);
    // Agrega otros assemblies seg�n necesites
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 1;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.CustomConfigureServices();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPermisosRepo, PermisosRepo>();
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
builder.Services.AddScoped<IFuncionalidadRepository, FuncionalidadRepository>();
builder.Services.AddScoped<IModuloService, ModuloService>();
builder.Services.AddScoped<IModuloRepository, ModuloRepository>();
builder.Services.AddScoped(typeof(GenericRepository<NNAs>));
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IFuncionalidadService, FuncionalidadService>();
builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
builder.Services.AddScoped<IGenericRepository<TPCIE10>, GenericRepository<TPCIE10>>();
builder.Services.AddScoped<INotificacionRepo, NotificacionRepo>();
builder.Services.AddScoped<IAlertaRepo, AlertaRepo>();
builder.Services.AddScoped<ISeguimientoRepo, SeguimientoRepo>();
builder.Services.AddScoped<IIntentoRepo, IntentoRepo>();
builder.Services.AddScoped<IDashboardRepo, DashboardRepo>();
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
builder.Services.AddScoped<IAuthRepo, AuthRepo>();
builder.Services.AddScoped<IUsurioRepo, UsuarioRepo>();


// Register Quartz services
builder.Services.AddSingleton<IJobFactory, SingletonJobFactory>();
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
builder.Services.AddSingleton<IJob, AsignacionAutomaticaJob>();
builder.Services.AddScoped<IEmailConfigurationRepo, EmailConfigurationRepo>();

var temporizadorAsignacionAutomatica = builder.Configuration.GetValue<string>("Quartz:AsignacionAutomaticaSeguimientos");

builder.Services.AddHostedService<QuartzHostedService>();

builder.Services.Configure<Core.DTOs.Quartz>(builder.Configuration.GetSection("Quartz"));

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
builder.Services.AddScoped<IReportesSIVIGILARepo, ReportesSIVIGILARepo>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<INotificacionRepo, NotificacionRepo>();


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