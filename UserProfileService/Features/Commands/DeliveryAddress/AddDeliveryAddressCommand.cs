using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Application;
using UserProfileService.Data;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.Commands.DeliveryAddress
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
          bool IsPrimary
      ) : IRequest<ApiResponse<DeliveryAddressResponse>>
    {
        public class Handler
            : IRequestHandler<AddDeliveryAddressCommand, ApiResponse<DeliveryAddressResponse>>
        {
            private readonly IUserProfileRepository _repository;
            private readonly ILogger<Handler> _logger;

            public Handler(
                IUserProfileRepository repository,
                ILogger<Handler> logger)
            {
                _repository = repository;
                _logger = logger;
            }

            public async Task<ApiResponse<DeliveryAddressResponse>> Handle(
                AddDeliveryAddressCommand request,
                CancellationToken cancellationToken)
            {
                try
                {
     
                    var userProfile = await _repository
                        .GetByUserIdAsync(request.UserId, cancellationToken);

                    if (userProfile == null)
                    {
                        return ApiResponse<DeliveryAddressResponse>
                            .Failure("Profile not found");
                    }

        
                    if (request.IsPrimary)
                    {
                        await _repository
                            .UnsetPrimaryAddressesAsync(userProfile.Id, cancellationToken);
                    }

                    var address = new Models.DeliveryAddress(
                        userProfile.Id,
                        request.Alias,
                        request.Street,
                        request.City,
                        request.Governorate,
                        request.Building,
                        request.Floor,
                        request.Apartment,
                        request.IsPrimary
                    );

            
                    await _repository.AddAddressAsync(address, cancellationToken);

                
                    await _repository.SaveChangesAsync(cancellationToken);

                    return ApiResponse<DeliveryAddressResponse>.Success(
                        new DeliveryAddressResponse
                        {
                            AddressId = address.Id,
                            CreatedAt = address.CreatedAt
                        });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Concurrency conflict while adding address for user {UserId}",
                        request.UserId);

                    return ApiResponse<DeliveryAddressResponse>.Failure(
                        "Profile was modified concurrently. Please retry.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while adding delivery address for user {UserId}",
                        request.UserId);

                    return ApiResponse<DeliveryAddressResponse>.Failure(
                        "Failed to add delivery address");
                }
            }
        }
    }
}