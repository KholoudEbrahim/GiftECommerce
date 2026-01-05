using MassTransit;
using System.Text.Json;
using UserProfileService.Data;
using UserProfileService.Models;
using Microsoft.EntityFrameworkCore;
namespace UserProfileService.Events
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly IUserProfileRepository _repository;
        private readonly UserProfileDbContext _dbContext;
        private readonly ILogger<UserCreatedConsumer> _logger;

        public UserCreatedConsumer(
            IUserProfileRepository repository,
             UserProfileDbContext dbContext,
            ILogger<UserCreatedConsumer> logger)
        {
            _repository = repository;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            _logger.LogInformation("=== USER CREATED EVENT RECEIVED ===");
            _logger.LogInformation($"Event: {JsonSerializer.Serialize(context.Message)}");

            try
            {
                var @event = context.Message;

                _logger.LogInformation(
                    "Processing UserCreatedEvent for user: {UserId} ({Email})",
                    @event.UserId, @event.Email);

                try
                {
                    var canConnect = await _dbContext.Database.CanConnectAsync();
                    _logger.LogInformation($"Database can connect: {canConnect}");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database connection failed");
                }

                var existingProfile = await _repository.GetByUserIdAsync(@event.UserId);
                if (existingProfile != null)
                {
                    _logger.LogWarning(
                        "Profile already exists for user: {UserId}. Skipping...",
                        @event.UserId);
                    return;
                }

                _logger.LogInformation("Creating new profile for user: {UserId}", @event.UserId);

                var profile = new UserProfile(
                    @event.UserId,
                    @event.FirstName ?? "User",
                    @event.LastName ?? "User");

                if (!string.IsNullOrEmpty(@event.Phone))
                {
                    profile.UpdateProfile(
                        @event.FirstName ?? "User",
                        @event.LastName ?? "User",
                        @event.Phone,
                        null);
                }

                await _repository.AddAsync(profile);
                await _repository.SaveChangesAsync();

                _logger.LogInformation(
                    "✓ Profile created successfully for user: {UserId}. Profile ID: {ProfileId}",
                    @event.UserId, profile.Id);

                _logger.LogInformation("=== EVENT PROCESSING COMPLETED ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "✗ Error processing UserCreatedEvent for user {UserId}",
                    context.Message?.UserId);
                throw;
            }
        }
    }
}



public class ProfileCreatedEvent
    {
        public Guid UserId { get; set; }
        public Guid ProfileId { get; set; }
        public DateTime CreatedAt { get; set; }
    }




    public class UserCreatedEvent
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

