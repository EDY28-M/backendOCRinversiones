using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace backendORCinverisones.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error no controlado");
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        string errorMessage;
        string userMessage;

        // ✅ Detectar errores de conexión a base de datos (solo errores REALES de conexión)
        if (exception is SqlException || 
            exception.InnerException is SqlException ||
            exception.Message.Contains("Cannot open database") ||
            exception.Message.Contains("A network-related") ||
            exception.Message.Contains("The server was not found"))
        {
            errorMessage = "Error de conexión a la base de datos";
            userMessage = "No se pudo conectar a la base de datos. Verifique la configuración del servidor.";
            _logger.LogError("❌ DATABASE CONNECTION ERROR: {Message}", exception.Message);
        }
        else
        {
            errorMessage = "Ocurrió un error interno en el servidor";
            userMessage = exception.Message; // Mostrar el mensaje real para debugging
            _logger.LogError("❌ SERVER ERROR: {Message} | InnerException: {Inner}", exception.Message, exception.InnerException?.Message);
        }

        // ✅ SEGURIDAD: Solo exponer detalles en desarrollo
        object responseObj;
        if (_env.IsDevelopment())
        {
            responseObj = new
            {
                error = errorMessage,
                message = exception.Message,
                timestamp = DateTime.UtcNow,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException?.Message
            };
        }
        else
        {
            responseObj = new
            {
                error = errorMessage,
                message = userMessage,
                timestamp = DateTime.UtcNow,
                hint = "Revise las variables de entorno ConnectionStrings__DefaultConnection en Render"
            };
        }

        var result = JsonSerializer.Serialize(responseObj);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}
