using Core.Interfaces.Repositorios;
using Infra.Repositories;
using Infra.Repositorios;
using MSSeguimiento.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.CustomConfigureServices();

builder.Services.AddScoped<INotificacionRepo, NotificacionRepo>();
builder.Services.AddScoped<IAlertaRepo, AlertaRepo>();
builder.Services.AddScoped<ISeguimientoRepo, SeguimientoRepo>();
builder.Services.AddScoped<IIntentoRepo, IntentoRepo>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder =>
        {
            _ = builder.WithOrigins("http://localhost:4200", "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyOrigin()
            .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
