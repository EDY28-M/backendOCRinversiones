using System.Threading;
using System.Threading.Tasks;

namespace backendORCinverisones.Application.Interfaces.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        bool isHtml = true,
        string? replyToEmail = null,
        string? replyToName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía un email usando una plantilla de Brevo con parámetros dinámicos
    /// </summary>
    Task<bool> SendTemplateEmailAsync(
        string toEmail,
        string toName,
        int templateId,
        Dictionary<string, object>? templateParams = null,
        string? replyToEmail = null,
        string? replyToName = null,
        CancellationToken cancellationToken = default);
}
