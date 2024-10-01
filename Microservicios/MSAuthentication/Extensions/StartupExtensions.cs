using Core.CQRS.MSUsuariosyRoles.Commands.User;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.MSPermisos;
using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Base;
using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Query.Base;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Modelos.Identity;
using Core.Services.MSUsuariosyRoles;
using Core.Validators.MSPermisos;
using Infra;
using Infra.Repositories;
using Infra.Repositorios.MSPermisos;
using Infra.Repositorios.MSUsuariosyRoles.Command.Base;
using Infra.Repositorios.MSUsuariosyRoles.Query.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MSAuthentication.Api.Extensions
{
    internal static class StartupExtensions
    {
        public static WebApplicationBuilder CustomConfigureServices(this WebApplicationBuilder builder)
        {
            //registrar el dbcontext y las interfaces

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
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

            // For authentication
            var _key = builder.Configuration["Jwt:Key"];
            var _issuer = builder.Configuration["Jwt:Issuer"];
            var _audience = builder.Configuration["Jwt:Audience"];
            var _expirtyMinutes = builder.Configuration["Jwt:ExpiryMinutes"];

            // Configuration for token
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = _audience,
                    ValidIssuer = _issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                    ClockSkew = TimeSpan.FromMinutes(Convert.ToDouble(_expirtyMinutes))

                };
            });


            //Registro de Repos
            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
            builder.Services.AddScoped<IPermisosRepo, PermisosRepo>();
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssignUsersRoleCommandHandler).Assembly));
            builder.Services.AddSingleton<ITokenGenerator>(new TokenGenerator(_key, _issuer, _audience, _expirtyMinutes));

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssignUsersRoleCommandHandler).Assembly));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost:4200", "https://secani-cbabfpddahe6ayg9.eastus-01.azurewebsites.net")
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials());
            });

            builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>()
                .AddCheck<CustomHealthCheck>("CustomHealthCheck");

            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped(typeof(IQueryRepository<>), typeof(QueryRepository<>));
            builder.Services.AddScoped(typeof(ICommandRepository<>), typeof(CommandRepository<>));
            return builder;
        }
    }
}
