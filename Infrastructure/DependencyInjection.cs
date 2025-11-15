using Application.Common.Interfaces;
using Infrastructure.Auth;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Persistence.Seeds;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext (SQL Server)
            var cs = configuration.GetConnectionString("DefaultConnection")!;
            services.AddDbContext<ApplicationDbContext>(opt =>
            {
                opt.UseSqlServer(cs, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
            });
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // MediatR (Application assembly)
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
                Assembly.Load("Application")));

            // JWT
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            var jwt = configuration.GetSection("Jwt").Get<JwtSettings>()!;
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                };
            });

            // Interceptors
            services.AddScoped<AuditingInterceptor>();
            services.AddScoped<SoftDeleteInterceptor>();
            services.AddScoped<TenantInterceptor>();
            services.AddScoped<IIdentityService, IdentityService>();


            // Helpers
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IFileStorage, LocalFileStorage>();

            return services;
        }

        public static async Task EnsureSeededAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await ctx.Database.MigrateAsync();
            await SeedData.SeedAsync(ctx);
        }
    }
}
