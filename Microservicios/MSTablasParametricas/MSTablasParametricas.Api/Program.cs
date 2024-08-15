using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios.Common;
using Core.Services.MSTablasParametricas;
using Infra;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));

//builder.Services.AddTransient<IEstadoAlertaRepository, EstadoAlertaRepository>();
//builder.Services.AddTransient<IEstadoNNARepository, EstadoNNARepository>();
//builder.Services.AddTransient<IEstadoSeguimientoRepository, EstadoSeguimientoRepository>();
//builder.Services.AddTransient<ITPCategoriaAlertaRepository, TPCategoriaAlertaRepository>();
//builder.Services.AddTransient<ITPSubCategoriaAlertaRepository, TPSubCategoriaAlertaRepository>();

//builder.Services.AddTransient<IEstadoAlertaService, EstadoAlertaService>();
//builder.Services.AddTransient<IEstadoNNAService, EstadoNNAService>();
//builder.Services.AddTransient<IEstadoSeguimientoService, EstadoSeguimientoService>();
//builder.Services.AddTransient<ITPCategoriaAlertaService, TPCategoriaAlertaService>();
//builder.Services.AddTransient<ITPSubCategoriaAlertaService, TPSubCategoriaAlertaService>();

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
