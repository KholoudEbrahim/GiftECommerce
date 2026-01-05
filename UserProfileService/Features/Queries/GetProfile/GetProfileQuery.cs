using MediatR;
using UserProfileService.Application;
using UserProfileService.Data;
using UserProfileService.Features.Shared;
using UserProfileService.Models;
using UserProfileService.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UserProfileService.Features.Queries.GetProfile
{
    public record GetUserProfileQuery(Guid UserId)
        : IRequest<ApiResponse<UserProfileDto>>
    {
        public class GetUserProfileHandler
            : IRequestHandler<GetUserProfileQuery, ApiResponse<UserProfileDto>>
        {
            private readonly IUserProfileRepository _userProfileRepository;
            private readonly IIdentityServiceClient _identityServiceClient;
            private readonly ILogger<GetUserProfileHandler> _logger;

            public GetUserProfileHandler(
                IUserProfileRepository userProfileRepository,
                IIdentityServiceClient identityServiceClient,
                ILogger<GetUserProfileHandler> logger)
            {
                _userProfileRepository = userProfileRepository;
                _identityServiceClient = identityServiceClient;
                _logger = logger;
            }

            public async Task<ApiResponse<UserProfileDto>> Handle(
                GetUserProfileQuery request,
                CancellationToken cancellationToken)
            {
                try
                {
                    _logger.LogInformation(
                        "Fetching profile for user {UserId}", request.UserId);

           var userProfile = await UserProfileFactory.GetOrCreateAsync(
                        request.UserId,
                        _userProfileRepository,
                        cancellationToken);

                    if (userProfile.CreatedAt == userProfile.UpdatedAt)
                    {
                        _logger.LogInformation(
                            "Profile created on-demand for user {UserId}", request.UserId);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Existing profile found for user {UserId}", request.UserId);
                    }

                    var identityInfo = await _identityServiceClient
                    .GetUserIdentityAsync(request.UserId, cancellationToken);

                    if (identityInfo == null)
                    {
                        _logger.LogWarning(
                            "Identity service unavailable or user not found for {UserId}. Returning profile data only.",
                            request.UserId);
                    }

                    var profileDto = new UserProfileDto
                    {
                        Id = userProfile.Id,
                        UserId = request.UserId,
                        FirstName = userProfile.FirstName,
                        LastName = userProfile.LastName,
                        Email = identityInfo?.Email,
                        PhoneNumber = userProfile.PhoneNumber
                             ?? identityInfo?.PhoneNumber,
                        ProfilePictureUrl = userProfile.ProfilePictureUrl,
                        CreatedAt = userProfile.CreatedAt,
                        UpdatedAt = userProfile.UpdatedAt,
                        DeliveryAddresses = userProfile.DeliveryAddresses
                            .Where(a => !a.IsDeleted)
                            .Select(MapToDeliveryAddressDto)
                            .ToList()
                    };

                    _logger.LogInformation(
                        "Profile retrieved successfully for user {UserId}", request.UserId);

                    return ApiResponse<UserProfileDto>.Success(profileDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while retrieving profile for user {UserId}",
                        request.UserId);

                    return ApiResponse<UserProfileDto>.Failure(
                        "An error occurred while retrieving user profile");
                }
            }

            private static DeliveryAddressDto MapToDeliveryAddressDto(
                DeliveryAddress address)
            {
                return new DeliveryAddressDto
                {
                    Id = address.Id,
                    Alias = address.Alias,
                    Street = address.Street,
                    City = address.City,
                    Governorate = address.Governorate,
                    Building = address.Building,
                    Floor = address.Floor,
                    Apartment = address.Apartment,
                    IsPrimary = address.IsPrimary,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                };
            }
        }
    }

}

