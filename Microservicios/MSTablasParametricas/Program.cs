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

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddControllers();

builder.Services.AddHttpClient<TablaParametricaService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
