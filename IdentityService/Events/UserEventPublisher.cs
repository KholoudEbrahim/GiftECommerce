using IdentityService.Models;
using MassTransit;

namespace IdentityService.Events
{
    public class UserEventPublisher : IUserEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<UserEventPublisher> _logger;

        public UserEventPublisher(
            IPublishEndpoint publishEndpoint,
            ILogger<UserEventPublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task PublishUserCreatedEventAsync(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                var @event = new UserCreatedEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Gender = user.Gender,
                    CreatedAt = DateTime.UtcNow
                };

                await _publishEndpoint.Publish(@event, cancellationToken);

                _logger.LogInformation(
                    " UserCreatedEvent published for user: {UserId} ({Email})",
                    user.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to publish UserCreatedEvent for user: {UserId}", user.Id);
               
            }
        }
    }
}
