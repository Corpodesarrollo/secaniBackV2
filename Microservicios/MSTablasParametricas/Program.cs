using Core.DTOs.MSTablasParametricas;
using Core.Interfaces;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Core.Services;
using Core.Services.MSTablasParametricas;
using Core.Services.StorageService;
using Core.Validators;
using FluentValidation;
using Infra;
using Infra.Repositories;
using Infra.Repositories.Common;
using Infra.Repositorios;
using Infra.Repositorios.MSTablasParametricas;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddCustomSwagger();

builder.Services.AddCustomAuthentication(true);

builder.Services.AddScoped<IAdjuntosRepo, AdjuntosRepo>();
builder.Services.AddScoped<IStorageService, StorageService>();

builder.Services.AddScoped<ICategoriaAlertaRepository, CategoriaAlertaRepository>();
builder.Services.AddScoped<ICategoriaAlertaService, CategoriaAlertaService>();
builder.Services.AddScoped<INNARepo, NNARepo>();
builder.Services.AddScoped<INombreTablaParametricaService, NombresTablaParametricaService>();
builder.Services.AddScoped<ITablaParametricaRepository, TablaParametricaRepository>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IContactoEntidadRepository, ContactoEntidadRepository>();
builder.Services.AddScoped<IContactoEntidadService, ContactoEntidadService>();

builder.Services.AddValidatorsFromAssemblyContaining<ContactoEntidadRequestValidator>();

builder.Services.AddScoped<IGenericService<TPEstadoSeguimiento, GenericTPDTO>, GenericService<TPEstadoSeguimiento, GenericTPDTO>>();
builder.Services.AddScoped<IGenericService<TPEstadoIngresoEstrategia, GenericTPDTO>, GenericService<TPEstadoIngresoEstrategia, GenericTPDTO>>();
builder.Services.AddScoped<IGenericService<TPOrigenReporte, GenericTPDTO>, GenericService<TPOrigenReporte, GenericTPDTO>>();

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

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");
app.UseCustomConfigure();
app.UseCustomSwagger();

app.Run();
