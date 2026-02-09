using System.Threading;
using System.Threading.Tasks;
using backendORCinverisones.Application.DTOs.Contact;

namespace backendORCinverisones.Application.Interfaces.Services;

public interface IContactService
{
    Task<(bool AdminSent, bool UserSent)> SendContactAsync(
        ContactRequestDto request,
        CancellationToken cancellationToken = default);
}
