using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Interfaces.Repositorios.MSPermisos;
using Core.Services;
using Core.Services.MSPermisos;
using Core.Validators;
using Core.Validators.MSPermisos;
using FluentValidation;
using Infra.Repositories;
using Infra.Repositories.Common;
using Infra.Repositorios.MSPermisos;
using MSAuthentication.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IFuncionalidadService, FuncionalidadService>();
builder.Services.AddTransient<IModuloService, ModuloService>();
builder.Services.AddTransient<IPermisoService, PermisoService>();

builder.Services.AddTransient<IFuncionalidadRepository, FuncionalidadRepository>();
builder.Services.AddTransient<IModuloRepository, ModuloRepository>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddTransient<IContactoEntidadRepository, ContactoEntidadRepository>();
builder.Services.AddTransient<IContactoEntidadService, ContactoEntidadService>();

builder.Services.AddMemoryCache();

builder.Services.AddValidatorsFromAssemblyContaining<PermisoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ContactoEntidadRequestValidator>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.CustomConfigureServices();

// Registrar IMemoryCache
builder.Services.AddMemoryCache();


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
