using Core.CQRS.MSUsuariosyRoles.Commands.User;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.MSPermisos;
using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Base;
using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Query.Base;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Modelos.Identity;
using Core.Services;
using Core.Services.MSPermisos;
using Core.Services.MSUsuariosyRoles;
using Core.Services.StorageService;
using Core.Validators;
using Core.Validators.MSPermisos;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infra;
using Infra.Repositories;
using Infra.Repositorios.MSPermisos;
using Infra.Repositorios.MSUsuariosyRoles.Command.Base;
using Infra.Repositorios.MSUsuariosyRoles.Query.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MSAuthentication.Api.Middleware;
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

//Registro de Repos
//registrar el dbcontext y las interfaces

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
));

//Registro de Repos
builder.Services.AddScoped<IFuncionalidadService, FuncionalidadService>();
builder.Services.AddScoped<IContactoEntidadRepository, ContactoEntidadRepository>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
builder.Services.AddScoped<IPermisosRepo, PermisosRepo>();
builder.Services.AddScoped<IModuloService, ModuloService>();
builder.Services.AddScoped<IModuloRepository, ModuloRepository>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IFuncionalidadRepository, FuncionalidadRepository>();
builder.Services.AddScoped<IContactoEntidadService, ContactoEntidadService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssignUsersRoleCommandHandler).Assembly));

builder.Services.AddTransient<IValidator<ContactoEntidadRequest>, ContactoEntidadRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

var _key = builder.Configuration["Jwt:Key"];
var _issuer = builder.Configuration["Jwt:Issuer"];
var _audience = builder.Configuration["Jwt:Audience"];
var _expirtyMinutes = builder.Configuration["Jwt:ExpiryMinutes"];
builder.Services.AddSingleton<ITokenGenerator>(new TokenGenerator(_key, _issuer, _audience, _expirtyMinutes));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>()
                .AddCheck<CustomHealthCheck>("CustomHealthCheck");

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));

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
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCustomConfigure();
app.UseCustomSwagger();

app.Run();
