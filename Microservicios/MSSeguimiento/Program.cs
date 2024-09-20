using Core.Interfaces.Repositorios;
using Infra.Repositories;
using Infra.Repositorios;
using MSSeguimiento.Api.Extensions;
using Quartz.Impl;
using Quartz.Spi;
using Quartz;

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
            _ = builder.WithOrigins("http://localhost:4200", "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
            .AllowAnyHeader()
            .AllowAnyOrigin()
            .AllowAnyMethod();
        });
});

//var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
//// Register Quartz services
//builder.Services.AddSingleton<IJobFactory, SingletonJobFactory>();
//builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
//builder.Services.AddSingleton<IJob, AsignacionAutomaticaJob>();

//string? temporizadorAsignacionAutomatica = builder.Configuration.GetValue<string>("Quartz:AsignacionAutomaticaSeguimientos");

//// Register the jobs and triggers
//builder.Services.AddSingleton<AsignacionAutomaticaJob>();
//builder.Services.AddSingleton(new JobSchedule(
//    jobType: typeof(AsignacionAutomaticaJob),
//    cronExpression: temporizadorAsignacionAutomatica,
//timeZone: timeZone));

//builder.Services.AddHostedService<QuartzHostedService>();

//builder.Services.Configure<Core.DTOs.Quartz>(builder.Configuration.GetSection("Quartz"));

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
