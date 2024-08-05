using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Base;
using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Query.Base;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Modelos.Identity;
using Core.Services.MSUsuariosyRoles;
using Infra;
using Infra.MSUsuariosyRoles;
using Infra.Repositorios.MSUsuariosyRoles.Command.Base;
using Infra.Repositorios.MSUsuariosyRoles.Query.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace UsuariosyRoles.Api.Extensions
{
    public static class StartupExtensions
    {
        public static WebApplicationBuilder CustomConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

            builder.Services.AddDbContext<ApplicationIdentityDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationIdentityDbContext).Assembly.FullName)
            ));


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false; // For special character
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                // Default SignIn settings.
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.User.RequireUniqueEmail = true;
            });

            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
            builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));

            return builder;
        }
    }
}
