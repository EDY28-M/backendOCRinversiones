using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using backendORCinverisones.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace backendORCinverisones.Application.Services;

/// <summary>
/// Servicio para envío de correos electrónicos mediante la API HTTP de Brevo
/// Endpoint: POST https://api.brevo.com/v3/smtp/email
/// Docs: https://developers.brevo.com/docs/send-a-transactional-email
/// </summary>
public class EmailService : IEmailService
{
    private const string BrevoApiUrl = "https://api.brevo.com/v3/smtp/email";

    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IWebHostEnvironment environment,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Envía un email con contenido HTML/texto estático via Brevo API
    /// </summary>
    public async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true,
        string? replyToEmail = null,
        string? replyToName = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var apiKey = GetBrevoApiKey();
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderName = _configuration["EmailSettings:SenderName"] ?? "ORC Inversiones";

        if (!ValidateConfig(apiKey, senderEmail, toEmail, subject))
            return SimulateIfDev(toEmail, subject);

        var payload = new Dictionary<string, object>
        {
            ["sender"] = new { name = senderName, email = senderEmail },
            ["to"] = new object[] { new { email = toEmail, name = toEmail } },
            ["subject"] = subject
        };

        if (isHtml)
            payload["htmlContent"] = body;
        else
            payload["textContent"] = body;

        if (!string.IsNullOrWhiteSpace(replyToEmail))
            payload["replyTo"] = new { email = replyToEmail, name = replyToName ?? replyToEmail };

        return await SendToBrevoAsync(payload, apiKey!, toEmail, cancellationToken);
    }

    /// <summary>
    /// Envía un email usando una plantilla de Brevo con parámetros dinámicos.
    /// El sender, subject y body se toman de la plantilla.
    /// </summary>
    public async Task<bool> SendTemplateEmailAsync(
        string toEmail,
        string toName,
        int templateId,
        Dictionary<string, object>? templateParams = null,
        string? replyToEmail = null,
        string? replyToName = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var apiKey = GetBrevoApiKey();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("⚠️ BrevoApiKey no configurada en EmailSettings");
            return SimulateIfDev(toEmail, $"Template #{templateId}");
        }

        var payload = new Dictionary<string, object>
        {
            ["to"] = new object[] { new { email = toEmail, name = toName } },
            ["templateId"] = templateId
        };

        if (templateParams != null && templateParams.Count > 0)
            payload["params"] = templateParams;

        if (!string.IsNullOrWhiteSpace(replyToEmail))
            payload["replyTo"] = new { email = replyToEmail, name = replyToName ?? replyToEmail };

        return await SendToBrevoAsync(payload, apiKey!, toEmail, cancellationToken);
    }

    // ─── Helpers ───────────────────────────────────────────

    /// <summary>
    /// Lee la API key de Brevo con fallback a env var directa.
    /// Evita el problema de template literals ${...} en appsettings.Production.json
    /// </summary>
    private string? GetBrevoApiKey()
    {
        var key = _configuration["EmailSettings:BrevoApiKey"];

        // Si el config tiene un template sin resolver (ej: "${BREVO_API_KEY}"), leer env var directo
        if (string.IsNullOrWhiteSpace(key) || key.StartsWith("${"))
            key = Environment.GetEnvironmentVariable("BREVO_API_KEY")
               ?? Environment.GetEnvironmentVariable("EmailSettings__BrevoApiKey");

        _logger.LogInformation("🔑 API Key Brevo (primeros 12 chars): {Prefix}...",
            key?.Length >= 12 ? key[..12] : "(vacía o muy corta)");

        return key;
    }

    private async Task<bool> SendToBrevoAsync(
        Dictionary<string, object> payload,
        string apiKey,
        string toEmail,
        CancellationToken cancellationToken)
    {
        try
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            _logger.LogInformation("📤 Enviando email via Brevo API a: {To}", toEmail);
            _logger.LogDebug("📤 Brevo payload: {Payload}", jsonPayload);

            using var httpClient = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, BrevoApiUrl);
            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Email enviado exitosamente a: {To} | Response: {Response}", toEmail, responseBody);
                return true;
            }

            _logger.LogError("❌ Brevo API error ({StatusCode}) a {To}: {Response}",
                (int)response.StatusCode, toEmail, responseBody);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                _logger.LogError("   💡 API Key inválida. Ve a Brevo > Settings > SMTP & API > pestaña 'API Keys'");
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                _logger.LogError("   💡 Verifica sender verificado en Brevo y que el templateId exista");

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar email via Brevo a {To}", toEmail);
            return false;
        }
    }

    private bool ValidateConfig(string? apiKey, string? senderEmail, string toEmail, string subject)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(senderEmail))
        {
            _logger.LogWarning("⚠️ EmailSettings no configurado (BrevoApiKey o SenderEmail faltante)");
            return false;
        }
        return true;
    }

    private bool SimulateIfDev(string toEmail, string subject)
    {
        if (_environment.IsDevelopment())
        {
            _logger.LogInformation("📧 [SIMULADO] Email para: {To} | Asunto: {Subject}", toEmail, subject);
            return true;
        }
        return false;
    }
}
