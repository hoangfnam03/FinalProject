using Infrastructure;
using Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.HttpLogging;
using QnA_BE.Swagger;

var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

// ===== CORS (DEV) =====
// 1) Đọc allow-list từ appsettings (nếu bạn muốn cố định vài origin)
// 2) Đồng thời cho phép mọi port của localhost/127.0.0.1 khi dev (đỡ chase port Live Server)
var configOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("DevCors", p =>
    {
        p.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin)) return false;

            // chặn origin không hợp lệ
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;

            // chỉ cho HTTP trong dev (Live Server thường là http)
            if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase)) return false;

            // allow mọi port cho localhost/127.0.0.1
            if (uri.Host == "localhost" || uri.Host == "127.0.0.1") return true;

            // fallback: nếu bạn có domain khác trong config
            return configOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
        // ❌ không AllowCredentials vì FE đang dùng Bearer token (không dùng cookie)
    });
});

// ===== Logging (tuỳ chọn) =====
builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields =
        HttpLoggingFields.RequestPropertiesAndHeaders |
        HttpLoggingFields.ResponsePropertiesAndHeaders;
});

// Infra DI
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ===== Middleware =====
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QnA API v1");
        c.DisplayRequestDuration();
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseHttpLogging();

app.UseRouting();

// CORS phải nằm giữa Routing và Auth
app.UseCors("DevCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// seed
await app.Services.EnsureSeededAsync();

app.Run();
