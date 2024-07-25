using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.Common;
using Core.Services;
using Core.Validators;
using FluentValidation;
using Infra;
using Infra.Repositories;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace MSEntidad.Api.Extensions
{
    internal static class StartupExtensions
    {
        public static WebApplicationBuilder CustomConfigureServices(this WebApplicationBuilder builder)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //registrar el dbcontext y las interfaces
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddTransient<IContactoEntidadRepository, ContactoEntidadRepository>();
            builder.Services.AddTransient<IContactoEntidadService, ContactoEntidadService>();

            builder.Services.AddValidatorsFromAssemblyContaining<ContactoEntidadRequestValidator>();

            builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
            {
                builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
            }));

            builder.Services.AddControllers();

            return builder;
        }

        public static void RegisterMiddlewares(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Requisitos.API v1");
                    c.DisplayRequestDuration();
                });
            }

            app.UseCors("corsapp");
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
