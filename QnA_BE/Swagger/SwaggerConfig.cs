using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace QnA_BE.Swagger
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "QnA API",
                    Version = "v1"
                });
                c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });

                // JWT Bearer
                var jwtScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập token theo dạng: Bearer {token}",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", jwtScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtScheme, Array.Empty<string>() }
                });

                // (Tuỳ chọn) đọc XML comments cho mô tả endpoint
                // var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xml));
            });

            return services;
        }
    }
}
