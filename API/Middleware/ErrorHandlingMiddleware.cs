using System.Net;
using System.Text.Json;

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

        // ✅ SEGURIDAD: Solo exponer detalles en desarrollo
        object responseObj;
        if (_env.IsDevelopment())
        {
            responseObj = new
            {
                error = "Ocurrió un error interno en el servidor",
                message = exception.Message,
                timestamp = DateTime.UtcNow,
                stackTrace = exception.StackTrace
            };
        }
        else
        {
            responseObj = new
            {
                error = "Ocurrió un error interno en el servidor",
                message = "Ha ocurrido un error. Por favor contacte al administrador.",
                timestamp = DateTime.UtcNow
            };
        }

        var result = JsonSerializer.Serialize(responseObj);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}
