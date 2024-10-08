using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Services.MSTablasParametricas;
using Infra;
using Infra.Repositories.Common;
using Infra.Repositorios;
using Infra.Repositorios.MSTablasParametricas;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
builder.Services.AddTransient<ICategoriaAlertaRepository, CategoriaAlertaRepository>();
builder.Services.AddTransient<ICategoriaAlertaService, CategoriaAlertaService>();
builder.Services.AddTransient<INNARepo, NNARepo>();
builder.Services.AddScoped<INombreTablaParametricaService, NombresTablaParametricaService>();
builder.Services.AddScoped<ITablaParametricaRepository, TablaParametricaRepository>();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

builder.Services.AddHttpClient<TablaParametricaService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder =>
        {
            _ = builder.WithOrigins("http://localhost:4200", "https://localhost:4200", "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
            .AllowAnyHeader()
            .AllowAnyOrigin()
            .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

app.UseCustomConfigure();
app.UseCustomSwagger();

app.Run();
