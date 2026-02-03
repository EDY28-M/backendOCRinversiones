using System.Text;
using System.IO.Compression;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using AspNetCoreRateLimit;
using backendORCinverisones.API.Middleware;
using backendORCinverisones.Application.Interfaces.Repositories;
using backendORCinverisones.Application.Interfaces.Services;
using backendORCinverisones.Application.Services;
using backendORCinverisones.Application.Mappings;
using backendORCinverisones.Application.Validators;
using backendORCinverisones.Infrastructure.Data;
using backendORCinverisones.Infrastructure.Repositories;

// ✅ SERILOG - Logging estructurado de alto rendimiento
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/backend-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// ✅ Usar Serilog como logger
builder.Host.UseSerilog();

// ✅ OPTIMIZED Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);

            // ✅ Query splitting para mejorar performance en Include()
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);

            // ✅ Command timeout extendido para queries pesadas
            sqlOptions.CommandTimeout(60);
        });

    // ✅ Solo en desarrollo: mostrar queries SQL
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }

    // ✅ Deshabilitar tracking global (mejora performance para reads)
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<INombreMarcaRepository, NombreMarcaRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IImageCompressionService, ImageCompressionService>();

// ✅ DAPPER - Queries de alto rendimiento
builder.Services.AddScoped<IDapperQueryService, DapperQueryService>();

// Memory Cache and Response Caching
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// ✅ RATE LIMITING - Protección contra DDoS y fuerza bruta
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100 // 100 requests por minuto por IP (general)
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Period = "5m",
            Limit = 5 // Solo 5 intentos de login cada 5 minutos (anti-brute force)
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/products/bulk-import",
            Period = "1h",
            Limit = 10 // Solo 10 bulk imports por hora
        }
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ✅ AUTOMAPPER - Mapeo automatizado de entidades a DTOs
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// ✅ FLUENTVALIDATION - Validaciones declarativas
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

// ✅ RESPONSE COMPRESSION - Compresión GZIP/Brotli
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddControllers();

// Swagger Configuration with JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backend ORC Inversiones API",
        Version = "v1",
        Description = "API Backend con Clean Architecture para gestión de usuarios, roles, productos y categorías"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ✅ CORS - Restringido a orígenes específicos (sin wildcards)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        var corsOrigins = builder.Configuration["CorsOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) 
                          ?? new[] { "http://localhost:5173", "https://frontedocrinversiones.onrender.com" };

        Log.Information("🌐 CORS configurado para: {Origins}", string.Join(", ", corsOrigins));

        policy.WithOrigins(corsOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS") // Agregado OPTIONS para preflight
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "Accept") // Agregado Accept
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains(); // Permite subdominio wildcard si es necesario
    });
});

var app = builder.Build();

// ✅ CORS DEBE IR PRIMERO - Antes de cualquier otro middleware
app.UseCors("AllowAll");

// ✅ Serilog request logging
app.UseSerilogRequestLogging();

// ✅ Rate Limiting - DESPUÉS de CORS para que preflight funcione
app.UseIpRateLimiting();

// Middleware de manejo de errores
app.UseMiddleware<ErrorHandlingMiddleware>();

// ✅ Swagger habilitado en TODOS los ambientes (producción incluido)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend ORC Inversiones API v1");
    c.RoutePrefix = "swagger";
});

// Comentado para desarrollo con ngrok (usa HTTP en red local)
// app.UseHttpsRedirection();

// ✅ Response Compression ANTES de Response Caching
app.UseResponseCompression();
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("🚀 Backend ORC Inversiones iniciado correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Error fatal al iniciar la aplicación");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
