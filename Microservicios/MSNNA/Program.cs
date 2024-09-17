using Core.Interfaces;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Services;
using Core.Services.MSTablasParametricas;
using Infra;
using Infra.Repositories.Common;
using Infra.Repositorios;
using Microsoft.EntityFrameworkCore;
using MSNNA.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));
builder.Services.AddControllers();

builder.Services.AddHttpClient<TablaParametricaService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:4200", "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", policy =>
{
    policy.AllowAnyHeader()
          .AllowAnyMethod()
          .AllowAnyOrigin();
}));


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();
