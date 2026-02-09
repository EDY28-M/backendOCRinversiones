using System.Net;
using System.Net.Mail;
using backendORCinverisones.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace backendORCinverisones.Application.Services;

/// <summary>
/// Servicio para envío de correos electrónicos mediante SMTP
/// </summary>
public class EmailService : IEmailService
{
    private const string PlaceholderPassword = "CHANGE_ME_APP_PASSWORD";
    private const string PlaceholderPasswordEs = "TU_CONTRASEÑA_DE_APLICACION_AQUI";

    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _environment;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _logger = logger;
        _environment = environment;
    }

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

        try
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"] ?? "ORC Inversiones";
            var password = _configuration["EmailSettings:Password"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            // Validar configuración básica
            if (string.IsNullOrWhiteSpace(smtpServer) || string.IsNullOrWhiteSpace(senderEmail))
            {
                _logger.LogWarning("⚠️ Email no configurado correctamente. Configure EmailSettings en appsettings.json");
                if (_environment.IsDevelopment())
                {
                    _logger.LogInformation("📧 [SIMULADO] Email para: {To}", toEmail);
                    _logger.LogInformation("📧 [SIMULADO] Asunto: {Subject}", subject);
                    return true;
                }

                return false;
            }

            password = ResolvePassword(password);

            if (IsMissingPassword(password))
            {
                if (_environment.IsDevelopment())
                {
                    _logger.LogWarning("⚠️ Email sin password. Configura EMAIL_PASSWORD (recomendado) o EmailSettings:Password.");
                    _logger.LogInformation("📧 [SIMULADO] Email para: {To}", toEmail);
                    _logger.LogInformation("📧 [SIMULADO] Asunto: {Subject}", subject);
                    _logger.LogInformation("📧 [SIMULADO] Para Gmail usa una 'Contraseña de aplicación' (2FA) y guárdala como secret/ENV.");
                    return true;
                }

                _logger.LogError("❌ Email no configurado: falta EmailSettings:Password / EMAIL_PASSWORD en entorno no-Development.");
                return false;
            }

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000,
                UseDefaultCredentials = false
            };

            if (smtpServer.Contains("gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                client.EnableSsl = true; // Gmail requiere TLS
            }

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(toEmail);

            if (!string.IsNullOrWhiteSpace(replyToEmail))
            {
                message.ReplyToList.Add(new MailAddress(replyToEmail, replyToName ?? replyToEmail));
            }

            await client.SendMailAsync(message);

            _logger.LogInformation("✅ Email enviado exitosamente a: {To}", toEmail);
            return true;
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError("❌ Error SMTP al enviar email a {To}: {Message}", toEmail, smtpEx.Message);
            _logger.LogError("   StatusCode: {StatusCode}", smtpEx.StatusCode);

            if (smtpEx.StatusCode == SmtpStatusCode.MustIssueStartTlsFirst)
            {
                _logger.LogError("   💡 Solución: Asegúrate de que EnableSsl esté en 'true' y uses el puerto 587 para Gmail");
            }
            else if (smtpEx.Message.Contains("Authentication", StringComparison.OrdinalIgnoreCase) ||
                     smtpEx.Message.Contains("5.7.0", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("   💡 Solución: Verifica que la contraseña de aplicación sea correcta");
                _logger.LogError("   💡 Para Gmail: Usa una 'Contraseña de aplicación', no tu contraseña normal");
                _logger.LogError("   💡 Pasos: Google Account -> Seguridad -> Verificación en 2 pasos -> Contraseñas de aplicación");
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al enviar email a {To}", toEmail);
            return false;
        }
    }

    private string? ResolvePassword(string? configuredPassword)
    {
        if (!IsMissingPassword(configuredPassword))
            return configuredPassword;

        return Environment.GetEnvironmentVariable("EMAIL_PASSWORD")
               ?? Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD")
               ?? configuredPassword;
    }

    private static bool IsMissingPassword(string? password)
    {
        return string.IsNullOrWhiteSpace(password) ||
               password == PlaceholderPassword ||
               password == PlaceholderPasswordEs;
    }
}
