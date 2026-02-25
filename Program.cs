using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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
using backendORCinverisones.Infrastructure.HealthChecks;

// ============================================
// ✅ SERILOG - Logging estructurado de alto rendimiento
// ============================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/backend-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// ✅ Agregar variables de entorno a la configuración
builder.Configuration.AddEnvironmentVariables();

// ✅ Usar Serilog como logger
builder.Host.UseSerilog();

// ============================================
// ✅ OPTIMIZACIÓN: Configuración de Kestrel
// ============================================
builder.WebHost.ConfigureKestrel(options =>
{
    // ✅ Render inyecta la variable PORT automáticamente
    var port = int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "10000");
    options.ListenAnyIP(port);

    // Aumentar límites de conexión para alta concurrencia
    options.Limits.MaxConcurrentConnections = 1000;
    options.Limits.MaxConcurrentUpgradedConnections = 1000;
    
    // Configurar timeouts
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
    
    // Habilitar compresión de respuestas grandes
    options.Limits.MaxResponseBufferSize = 64 * 1024; // 64KB
    options.Limits.MaxRequestBufferSize = 64 * 1024;  // 64KB
});

// ============================================
// ✅ DATABASE CONFIGURATION - EF Core Optimizado
// ============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Log.Information("🗄️ Base de datos configurada");

builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            // Retry policy para resiliencia
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);

            // ✅ Query splitting para mejorar performance en Include()
            sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);

            // Command timeout extendido para queries pesadas
            sqlOptions.CommandTimeout(120);
        });

    // Solo en desarrollo: mostrar queries SQL
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }

    // ✅ Deshabilitar tracking global (mejora performance para reads)
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    
    // ✅ Agregar interceptor de performance
    options.AddInterceptors(serviceProvider.GetRequiredService<DatabasePerformanceInterceptor>());
});

// ============================================
// ✅ REGISTRO DE SERVICIOS
// ============================================

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
builder.Services.AddSingleton<HybridCacheService>();
builder.Services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<HybridCacheService>());
builder.Services.AddScoped<IImageCompressionService, ImageCompressionService>();
builder.Services.AddScoped<IDapperQueryService, DapperQueryService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IContactService, ContactService>();

// ✅ WARMUP: Pre-calentar DB, EF Core y BCrypt al iniciar la app (reduce cold start)
builder.Services.AddHostedService<backendORCinverisones.Infrastructure.WarmupService>();

// Interceptores
builder.Services.AddSingleton<DatabasePerformanceInterceptor>();

// Memory Cache y Response Caching
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// HttpClient para Brevo API
builder.Services.AddHttpClient();

// ============================================
// ✅ RATE LIMITING - Protección contra abuso
// ============================================
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
            Limit = 500 // 500 requests por minuto por IP (admin panel hace muchas calls)
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Period = "1m",
            Limit = 20 // 20 intentos de login por minuto
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/products/bulk-import",
            Period = "1h",
            Limit = 50 // 50 bulk imports por hora
        }
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// ============================================
// ✅ JWT AUTHENTICATION
// ============================================
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

// ============================================
// ✅ AUTOMAPPER
// ============================================
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// ============================================
// ✅ FLUENTVALIDATION
// ============================================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

// ============================================
// ✅ RESPONSE COMPRESSION - Brotli + GZIP
// ============================================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "text/plain", "text/css", "application/javascript" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.AddControllers();

// ============================================
// ✅ HEALTH CHECKS
// ============================================
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "sql" })
    .AddCheck<CacheHealthCheck>("cache", tags: new[] { "cache", "memory" })
    .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "memory", "system" });

// ============================================
// ✅ SWAGGER CONFIGURATION
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Backend ORC Inversiones API",
        Version = "v1",
        Description = "API Backend con Clean Architecture para gestión de usuarios, roles, productos y categorías",
        Contact = new OpenApiContact
        {
            Name = "ORC Inversiones",
            Email = "support@orcinversionesperu.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

// ============================================
// ✅ CORS CONFIGURATION
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        // Leer orígenes desde env var (sin templates ${...})
        var rawOrigins = Environment.GetEnvironmentVariable("CorsOrigins")
                      ?? builder.Configuration["CorsOrigins"];

        // Si el valor es un template sin resolver o está vacío, usar los conocidos
        var corsOrigins = (!string.IsNullOrWhiteSpace(rawOrigins) && !rawOrigins.StartsWith("${"))
            ? rawOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : new[]
            {
                "http://localhost:5173",
                "http://localhost:5174",
                "https://frontedocrinversiones.onrender.com",
                "https://orcinversionesperu.com",
                "https://www.orcinversionesperu.com",
                "https://backendocrinversiones.onrender.com"
            };

        Log.Information("🌐 CORS configurado para: {Origins}", string.Join(", ", corsOrigins));

        policy.WithOrigins(corsOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "Accept")
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// ============================================
// ✅ BUILD APPLICATION
// ============================================
var app = builder.Build();

// ✅ CORS PRIMERO - Antes de cualquier otro middleware
app.UseCors("AllowAll");

// ✅ Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
});

// ✅ Rate Limiting - DESPUÉS de CORS
app.UseIpRateLimiting();

// ✅ Middleware de manejo de errores
app.UseMiddleware<ErrorHandlingMiddleware>();

// ✅ Swagger en todos los ambientes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend ORC Inversiones API v1");
    c.RoutePrefix = "swagger";
    c.DefaultModelsExpandDepth(-1); // Colapsar modelos por defecto
});

// ✅ Response Compression ANTES de Response Caching
app.UseResponseCompression();
app.UseResponseCaching();

// ✅ Health Checks endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration.TotalMilliseconds,
            Checks = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration.TotalMilliseconds,
                Description = e.Value.Description,
                Data = e.Value.Data
            })
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { Status = report.Status.ToString() });
    }
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Solo verifica que la app esté viva
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================================
// ✅ START APPLICATION
// ============================================
try
{
    Log.Information("🚀 Backend ORC Inversiones iniciado correctamente");
    Log.Information("📊 Health checks disponibles en: /health, /health/ready, /health/live");
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
