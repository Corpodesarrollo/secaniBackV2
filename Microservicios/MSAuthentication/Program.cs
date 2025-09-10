using Core.Common;
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
using Core.Validators.MSPermisos;
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
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MSEntidad.Api.Extensions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SISPRO.TRV.Entity.Exceptions;
using SISPRO.TRV.Entity.Helpers;
using SISPRO.TRV.General;
using SISPRO.TRV.General.Helpers;
using SISPRO.TRV.Web.MVCCore;
using SISPRO.TRV.Web.MVCCore.Extensions;
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
        options.JsonSerializerOptions.Converters.Add(new ByteArrayConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Configurar routing para permitir OPTIONS globalmente
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Asegurar que CORS maneje OPTIONS automáticamente
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins(
            "http://192.168.152.17:8140",
            "https://secani.sispropreprod.gov.co",
            "https://nna.sispropreprod.gov.co", // Agregando el dominio de la API también
            "http://192.168.110.11:8140",
            "http://localhost:4200",
            "https://localhost:4200",
            "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
                          .AllowAnyMethod()  // Esto incluye OPTIONS automáticamente
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromMinutes(30))); // Cache preflight por 30 min
});

builder.Services.AddCustomSwagger();

builder.Services.AddCustomAuthentication(true);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(GetUserQuery).Assembly);
    // Agrega otros assemblies según necesites
});

builder.Services.AddSingleton<ITokenGenerator>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new TokenGenerator(
        config["JwtSettings:Secret"],
        config["JwtSettings:Issuer"],
        config["JwtSettings:Audience"],
        config["JwtSettings:ExpiryMinutes"]);
});
builder.CustomConfigureServices();
builder.Services.AddScoped<IPermisosRepo, PermisosRepo>();
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
builder.Services.AddScoped<IFuncionalidadRepository, FuncionalidadRepository>();
builder.Services.AddScoped<IModuloService, ModuloService>();
builder.Services.AddScoped<IModuloRepository, ModuloRepository>();
builder.Services.AddScoped(typeof(GenericRepository<NNAs>));
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IFuncionalidadService, FuncionalidadService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
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

// Register the jobs and triggers
//builder.Services.AddSingleton<AsignacionAutomaticaJob>();
//builder.Services.AddSingleton(new JobSchedule(
//    jobType: typeof(AsignacionAutomaticaJob),
//    cronExpression: temporizadorAsignacionAutomatica,
//timeZone: timeZone));

builder.Services.AddHostedService<QuartzHostedService>();

builder.Services.Configure<Core.DTOs.Quartz>(builder.Configuration.GetSection("Quartz"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Solo dígitos
    options.Password.RequireDigit = false;             // Requiere al menos un dígito
    options.Password.RequireLowercase = false;        // No requiere minúsculas
    options.Password.RequireUppercase = false;        // No requiere mayúsculas
    options.Password.RequireNonAlphanumeric = false;  // No requiere símbolos
    options.Password.RequiredLength = 3;              // Mínimo 3 caracteres
    options.Password.RequiredUniqueChars = 0;         // Al menos un carácter único
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
builder.Services.AddScoped<IReportesSIVIGILARepo, ReportesSIVIGILARepo>();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>()
                .AddCheck<CustomHealthCheck>("CustomHealthCheck");

WebApplication app = builder.Build();

app.SetLogger();
ReadConfig.SetCultures();
app.UseHsts();
app.UseExceptionHandler(delegate (IApplicationBuilder errorApp)
{
    errorApp.Run(async delegate (HttpContext pContext)
    {
        Exception ex = pContext.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        SISPRO.TRV.General.Log.Error(ex);
        pContext.Response.StatusCode = (int)ex.GetHttpStatusCode();
        RequestHeaders reqHeaders = pContext.Request.GetTypedHeaders();
        if (ex.GetBaseException() is UserSessionIsClosedException)
        {
            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Domain = ReadConfig.PageDomain,
                SameSite = SameSiteMode.Lax
            };
            pContext.Response.Cookies.Delete(ReadConfig.TicketName, cookieOptions);
        }

        string messageContents;
        if (reqHeaders.AcceptJSONFirst())
        {
            pContext.Response.ContentType = "application/json";
            messageContents = ex.GetBasicErrorMessage().SerializeJSON();
        }
        else if (reqHeaders.AcceptXMLFirst())
        {
            pContext.Response.ContentType = "application/xml";
            messageContents = SerializeHelper.SerializeXML(ex.GetBasicErrorMessage(), false, true, true, null);
        }
        else if (reqHeaders.AcceptHTMLFirst())
        {
            pContext.Response.ContentType = "text/html";
            messageContents = ex.GetBasicErrorMessageHTML();
        }
        else
        {
            pContext.Response.ContentType = "text/plain";
            messageContents = ex.GetClientExtendedMessage();
        }

        await pContext.Response.WriteAsync(messageContents);
    });
});
app.UseRouting();
app.UseRequestLocalization();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseForwardedHeaders();

// DEBUG: Middleware para logging de CORS
app.Use(async (context, next) =>
{
    Console.WriteLine($"=== CORS DEBUG ===");
    Console.WriteLine($"Method: {context.Request.Method}");
    Console.WriteLine($"Path: {context.Request.Path}");
    Console.WriteLine($"Origin: {context.Request.Headers["Origin"]}");
    Console.WriteLine($"Access-Control-Request-Method: {context.Request.Headers["Access-Control-Request-Method"]}");
    Console.WriteLine($"Access-Control-Request-Headers: {context.Request.Headers["Access-Control-Request-Headers"]}");

    await next();

    Console.WriteLine($"Response Status: {context.Response.StatusCode}");
    Console.WriteLine($"CORS Headers: {string.Join(", ", context.Response.Headers.Where(h => h.Key.StartsWith("Access-Control")).Select(h => $"{h.Key}:{h.Value}"))}");
    Console.WriteLine($"=================");
});

// Middleware global para manejar OPTIONS (SOLO como fallback si CORS no funciona)
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        Console.WriteLine("=== OPTIONS REQUEST INTERCEPTED ===");
        // Dejar que CORS maneje primero
        await next();

        // Si CORS no manejó (status 404), manejar manualmente
        if (context.Response.StatusCode == 404)
        {
            Console.WriteLine("CORS didn't handle OPTIONS, handling manually");
            context.Response.StatusCode = 200;
            context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"].ToString());
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            return;
        }
    }
    else
    {
        await next();
    }
});

app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.StatusCode = 204; // No Content
        context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        return;
    }

    await next();
});
app.UseEndpoints(delegate (IEndpointRouteBuilder endpoints)
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks("/Health", new HealthCheckOptions
    {
        AllowCachingResponses = false,
        ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = 200,
                    [HealthStatus.Degraded] = 200,
                    [HealthStatus.Unhealthy] = 503
                }
    }).AllowAnonymous();
});
app.UseCustomSwagger();

app.UseStaticFiles();

app.Run();
