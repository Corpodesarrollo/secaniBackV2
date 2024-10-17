using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Base;
using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Query.Base;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Modelos.Identity;
using Core.Services.MSUsuariosyRoles;
using Infra;
using Infra.Repositories;
using Infra.Repositorios;
using Infra.Repositorios.MSUsuariosyRoles.Command.Base;
using Infra.Repositorios.MSUsuariosyRoles.Query.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSSeguimiento.Api.Extensions;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SISPRO.TRV.General;
using SISPRO.TRV.Web.MVCCore.Helpers;
using SISPRO.TRV.Web.MVCCore.StartupExtensions;

WebApplicationBuilder builder = WebApplicationHelper.CreateCustomBuilder<Program>(args);

ReadConfig.FixLoadAppSettings(builder.Configuration);

builder.Services.AddCustomConfigureServicesPreviousMvc();
builder
    .Services
    .AddCustomMvcControllers()
    .AddJsonOptions();

builder.Services.AddCustomSwagger();

builder.Services.AddCustomAuthentication(true);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// Registro de los servicios
builder.CustomConfigureServices();

builder.Services.AddScoped<INotificacionRepo, NotificacionRepo>();
builder.Services.AddScoped<IAlertaRepo, AlertaRepo>();
builder.Services.AddScoped<ISeguimientoRepo, SeguimientoRepo>();
builder.Services.AddScoped<IIntentoRepo, IntentoRepo>();
builder.Services.AddScoped<IDashboardRepo, DashboardRepo>();

var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
// Register Quartz services
builder.Services.AddSingleton<IJobFactory, SingletonJobFactory>();
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
builder.Services.AddSingleton<IJob, AsignacionAutomaticaJob>();

var temporizadorAsignacionAutomatica = builder.Configuration.GetValue<string>("Quartz:AsignacionAutomaticaSeguimientos");

// Register the jobs and triggers
builder.Services.AddSingleton<AsignacionAutomaticaJob>();
builder.Services.AddSingleton(new JobSchedule(
    jobType: typeof(AsignacionAutomaticaJob),
    cronExpression: temporizadorAsignacionAutomatica,
timeZone: timeZone));

builder.Services.AddHostedService<QuartzHostedService>();

builder.Services.Configure<Core.DTOs.Quartz>(builder.Configuration.GetSection("Quartz"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://192.168.110.11:8140", "http://localhost:4200", "https://localhost:4200", "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));

WebApplication app = builder.Build();

app.UseCors("AllowSpecificOrigin");

app.UseCustomConfigure();
app.UseCustomSwagger();
app.UseStaticFiles();

app.Run();
