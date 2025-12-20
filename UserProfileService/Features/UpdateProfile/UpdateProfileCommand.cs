using MediatR;
using UserProfileService.Data;
using UserProfileService.Features.Shared;
using UserProfileService.Models;

namespace UserProfileService.Features.UpdateProfile
{
    public record UpdateProfileCommand(
     Guid UserId,
     string FirstName,
     string LastName,
     string? PhoneNumber,
     DateTime? DateOfBirth,
     string? ProfilePictureUrl) : IRequest<ApiResponse<UpdatedProfileResponse>>
    {
        public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, ApiResponse<UpdatedProfileResponse>>
        {
            private readonly IUserProfileRepository _userProfileRepository;
            private readonly ILogger<UpdateProfileHandler> _logger;

            public UpdateProfileHandler(
                IUserProfileRepository userProfileRepository,
                ILogger<UpdateProfileHandler> logger)
            {
                _userProfileRepository = userProfileRepository;
                _logger = logger;
            }

            public async Task<ApiResponse<UpdatedProfileResponse>> Handle(
                UpdateProfileCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    // Check if profile exists
                    var existingProfile = await _userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);

                    if (existingProfile == null)
                    {
                        // Create new profile if it doesn't exist
                        existingProfile = new UserProfile(
                            request.UserId,
                            request.FirstName,
                            request.LastName);

                        existingProfile.UpdateProfile(
                            request.FirstName,
                            request.LastName,
                            request.PhoneNumber,
                            request.DateOfBirth);

                        await _userProfileRepository.AddAsync(existingProfile, cancellationToken);
                    }
                    else
                    {
                        // Update existing profile
                        existingProfile.UpdateProfile(
                            request.FirstName,
                            request.LastName,
                            request.PhoneNumber,
                            request.DateOfBirth);

                        await _userProfileRepository.UpdateAsync(existingProfile, cancellationToken);
                    }

                    await _userProfileRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Profile updated for user {UserId}", request.UserId);

                    return ApiResponse<UpdatedProfileResponse>.Success(new UpdatedProfileResponse
                    {
                        ProfileId = existingProfile.Id,
                        UpdatedAt = existingProfile.UpdatedAt
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating profile for user {UserId}", request.UserId);
                    return ApiResponse<UpdatedProfileResponse>.Failure("An error occurred while updating profile");
                }
            }
        }

    }
}
