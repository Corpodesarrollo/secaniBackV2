using Core.Validators.MSPermisos;
using Infra;
using MSPermisos.Api.Middleware;
using MSPermisos.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.CustomConfigureServices();

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder
    .WithOrigins("*")
    .AllowAnyMethod()
    .AllowAnyHeader();
}));

// Add Health Checks
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>()
    .AddCheck<CustomHealthCheck>("CustomHealthCheck");

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
