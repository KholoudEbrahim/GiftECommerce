using MediatR;
using UserProfileService.Data;
using UserProfileService.Features.Shared;
using UserProfileService.Services;
using UserProfileService.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UserProfileService.Features.Queries.GetProfile
{
    public record GetUserProfileQuery(Guid UserId) : IRequest<ApiResponse<UserProfileDto>>
    {
        public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, ApiResponse<UserProfileDto>>
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
                
                    var userProfile = await _userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);

               
                    var identityInfo = await _identityServiceClient.GetUserIdentityAsync(request.UserId, cancellationToken);

                    if (identityInfo == null)
                    {
                        return ApiResponse<UserProfileDto>.Failure("User not found in identity service");
                    }

            
                    var profileDto = new UserProfileDto
                    {
                        UserId = request.UserId,
                        Email = identityInfo.Email,
                        PhoneNumber = identityInfo.PhoneNumber, 
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    if (userProfile != null)
                    {

                        profileDto.Id = userProfile.Id;
                        profileDto.FirstName = userProfile.FirstName;
                        profileDto.LastName = userProfile.LastName;
                        profileDto.PhoneNumber = userProfile.PhoneNumber ?? identityInfo.PhoneNumber;
                        profileDto.ProfilePictureUrl = userProfile.ProfilePictureUrl;
                        profileDto.CreatedAt = userProfile.CreatedAt;
                        profileDto.UpdatedAt = userProfile.UpdatedAt;

                        // Map delivery addresses
                        profileDto.DeliveryAddresses = userProfile.DeliveryAddresses
                            .Select(MapToDeliveryAddressDto)
                            .ToList();
                    }
                    else
                    {
                 
                        profileDto.Id = Guid.NewGuid();
                        profileDto.FirstName = identityInfo.FirstName;
                        profileDto.LastName = identityInfo.LastName;
                     
                    }

                    return ApiResponse<UserProfileDto>.Success(profileDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting user profile for user {UserId}", request.UserId);
                    return ApiResponse<UserProfileDto>.Failure("An error occurred while retrieving user profile");
                }
            }

            private static DeliveryAddressDto MapToDeliveryAddressDto(DeliveryAddress address)
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

