using IdentityService.Models;

namespace IdentityService.Events
{
    public interface IUserEventPublisher
    {
        Task PublishUserCreatedEventAsync(User user, CancellationToken cancellationToken = default);
    }
}
