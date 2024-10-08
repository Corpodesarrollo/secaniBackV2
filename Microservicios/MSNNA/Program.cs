using Core.Interfaces;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Services;
using Core.Services.MSTablasParametricas;
using Infra.Repositories.Common;
using Infra.Repositorios;
using MSNNA.Api.Extensions;
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

// Registro de los servicios
builder.CustomConfigureServices();

//Registro de Repos
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));

builder.Services.AddScoped<IContactoNNARepo, ContactoNNARepo>();
builder.Services.AddTransient<IContactoNNARepo, ContactoNNARepo>();
builder.Services.AddScoped<IContactoNNAService, ContactoNNAService>();
builder.Services.AddTransient<IContactoNNAService, ContactoNNAService>();


builder.Services.AddScoped<INNARepo, NNARepo>();
builder.Services.AddTransient<INNARepo, NNARepo>();
builder.Services.AddScoped<INNAService, NNAService>();
builder.Services.AddTransient<INNAService, NNAService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://192.168.110.11:8140", "http://localhost:4200", "https://localhost:4200", "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

WebApplication app = builder.Build();

app.UseCors("AllowSpecificOrigin");

app.UseCustomConfigure();
app.UseCustomSwagger();

app.Run();
