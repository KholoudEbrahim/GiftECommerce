using MediatR;
using UserProfileService.Data;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.DeliveryAddress
{
    public record AddDeliveryAddressCommand(
       Guid UserId,
       string Alias,
       string Street,
       string City,
       string Governorate,
       string Building,
       string? Floor,
       string? Apartment,
       bool IsPrimary) : IRequest<ApiResponse<DeliveryAddressResponse>>
    {
        public class AddDeliveryAddressHandler : IRequestHandler<AddDeliveryAddressCommand, ApiResponse<DeliveryAddressResponse>>
        {
            private readonly IUserProfileRepository _userProfileRepository;
            private readonly ILogger<AddDeliveryAddressHandler> _logger;

            public AddDeliveryAddressHandler(
                IUserProfileRepository userProfileRepository,
                ILogger<AddDeliveryAddressHandler> logger)
            {
                _userProfileRepository = userProfileRepository;
                _logger = logger;
            }

            public async Task<ApiResponse<DeliveryAddressResponse>> Handle(
                AddDeliveryAddressCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
                    // Get user profile
                    var userProfile = await _userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);

                    if (userProfile == null)
                    {
                        return ApiResponse<DeliveryAddressResponse>.Failure("User profile not found");
                    }

                    // Add delivery address
                    var deliveryAddress = userProfile.AddDeliveryAddress(
                        request.Alias,
                        request.Street,
                        request.City,
                        request.Governorate,
                        request.Building,
                        request.Floor,
                        request.Apartment,
                        request.IsPrimary);

                    await _userProfileRepository.UpdateAsync(userProfile, cancellationToken);
                    await _userProfileRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Delivery address added for user {UserId}, address ID: {AddressId}",
                        request.UserId, deliveryAddress.Id);

                    return ApiResponse<DeliveryAddressResponse>.Success(new DeliveryAddressResponse
                    {
                        AddressId = deliveryAddress.Id,
                        CreatedAt = deliveryAddress.CreatedAt
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding delivery address for user {UserId}", request.UserId);
                    return ApiResponse<DeliveryAddressResponse>.Failure("An error occurred while adding delivery address");
                }
            }
        }
    }
   
}
