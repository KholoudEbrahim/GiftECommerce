using MediatR;
using UserProfileService.Data;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.Commands.RemoveDeliveryAddress
{
    public record RemoveDeliveryAddressCommand(
       Guid UserId,
       Guid AddressId) : IRequest<ApiResponse>;

    public class RemoveDeliveryAddressCommandHandler : IRequestHandler<RemoveDeliveryAddressCommand, ApiResponse>
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ILogger<RemoveDeliveryAddressCommandHandler> _logger;

        public RemoveDeliveryAddressCommandHandler(
            IUserProfileRepository userProfileRepository,
            ILogger<RemoveDeliveryAddressCommandHandler> logger)
        {
            _userProfileRepository = userProfileRepository;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(
            RemoveDeliveryAddressCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
             
                var userProfile = await _userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);

                if (userProfile == null)
                {
                    return ApiResponse.Failure("User profile not found");
                }

           
                var addressToRemove = userProfile.DeliveryAddresses
                    .FirstOrDefault(a => a.Id == request.AddressId && !a.IsDeleted);

                if (addressToRemove == null)
                {
                    return ApiResponse.Failure("Address not found or already deleted");
                }

                if (addressToRemove.IsPrimary)
                {
                    return ApiResponse.Failure("Cannot delete primary address. Please set another address as primary first.");
                }

            
                addressToRemove.IsDeleted = true;
         
                await _userProfileRepository.UpdateAsync(userProfile, cancellationToken);
                await _userProfileRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Soft deleted address {AddressId} for user {UserId}",
                    request.AddressId, request.UserId);

                return ApiResponse.Success(new { Message = "Address deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for user {UserId}",
                    request.AddressId, request.UserId);
                return ApiResponse.Failure("An error occurred while deleting address");
            }
        }
    }

}
