using Core.Interfaces.Repositorios;
using Core.Models;
using Infra.Repositories;
using Infra.Repositorios;
using MSAuthentication.Api.Extensions;
using SISPRO.TRV.General;
using SISPRO.TRV.Web.MVCCore.Helpers;
using SISPRO.TRV.Web.MVCCore.StartupExtensions;

WebApplicationBuilder builder = WebApplicationHelper.CreateCustomBuilder<Program>(args);

ReadConfig.FixLoadAppSettings(builder.Configuration);

builder.Services.AddCustomConfigureServicesPreviousMvc();
builder
    .Services
    .AddCustomMvcControllers()
    .AddJsonOptions()
    .AddFluentValidation<Greetings_SayHi_RequestValidator>();

builder.Services.AddCustomSwagger();

builder.Services.AddCustomAuthentication(true);

// Registro de los servicios
builder.CustomConfigureServices();

//Registro de Repos
builder.Services.AddScoped<IPermisosRepo, PermisosRepo>();
builder.Services.AddScoped<IAuthRepo, AuthRepo>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://localhost:4200", "http://localhost:4200", "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

WebApplication app = builder.Build();

app.UseCors("AllowSpecificOrigin");

app.UseCustomConfigure();
app.UseCustomSwagger();

app.Run();


///////////////////////////////////////////

//WebApplicationBuilder builder = WebApplicationHelper.CreateCustomBuilder<Program>(args);

//ReadConfig.FixLoadAppSettings(builder.Configuration);

//builder.Services.AddCustomConfigureServicesPreviousMvc();
//builder
//    .Services
//    .AddCustomMvcControllers()
//    .AddJsonOptions()
//    .AddFluentValidation<Greetings_SayHi_RequestValidator>();

//builder.Services.AddCustomAuthentication(true);

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//builder.CustomConfigureServices();

//// Registrar IMemoryCache
//builder.Services.AddMemoryCache();

////Registro de Repos
//builder.Services.AddScoped<IPermisosRepo, PermisosRepo>();


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowSpecificOrigin",
//        builder => builder.WithOrigins("http://localhost:4200", "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
//                          .AllowAnyMethod()
//                          .AllowAnyHeader()
//                          .AllowCredentials());
//});

//var app = builder.Build();

//// Configure the HTTP request pipeline.
////if (app.Environment.IsDevelopment())
////{
////    app.UseSwagger();
////    app.UseSwaggerUI();
////}

//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseCustomConfigure();

//app.UseCors("AllowSpecificOrigin");

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
