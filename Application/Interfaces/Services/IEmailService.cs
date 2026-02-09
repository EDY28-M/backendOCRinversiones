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
}
