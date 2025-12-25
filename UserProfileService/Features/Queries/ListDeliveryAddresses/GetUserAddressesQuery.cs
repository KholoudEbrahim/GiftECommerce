using MediatR;
using UserProfileService.Data;
using UserProfileService.Features.Shared;

namespace UserProfileService.Features.Queries.ListDeliveryAddresses
{
    public record GetUserAddressesQuery(
         Guid UserId,
         int PageNumber = 1,
         int PageSize = 10) : IRequest<ApiResponse<PaginatedResponse<AddressDto>>>;

    public class GetUserAddressesQueryHandler : IRequestHandler<GetUserAddressesQuery, ApiResponse<PaginatedResponse<AddressDto>>>
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ILogger<GetUserAddressesQueryHandler> _logger;

        public GetUserAddressesQueryHandler(
            IUserProfileRepository userProfileRepository,
            ILogger<GetUserAddressesQueryHandler> logger)
        {
            _userProfileRepository = userProfileRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedResponse<AddressDto>>> Handle(
            GetUserAddressesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
             
                if (request.PageNumber < 1)
                {
                    return ApiResponse<PaginatedResponse<AddressDto>>.Failure("Page number must be greater than 0");
                }

                if (request.PageSize < 1 || request.PageSize > 100)
                {
                    return ApiResponse<PaginatedResponse<AddressDto>>.Failure("Page size must be between 1 and 100");
                }

              
                var userProfile = await _userProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);

                if (userProfile == null)
                {
                    return ApiResponse<PaginatedResponse<AddressDto>>.Failure("User profile not found");
                }

                var activeAddresses = userProfile.DeliveryAddresses
                    .Where(a => !a.IsDeleted)
                    .ToList();

               
                var totalCount = activeAddresses.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                var pagedAddresses = activeAddresses
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

          
                var addressDtos = pagedAddresses.Select(address => new AddressDto
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
                }).ToList();

                var response = new PaginatedResponse<AddressDto>
                {
                    Items = addressDtos,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages
                };

                _logger.LogInformation(
                    "Retrieved {Count} addresses for user {UserId}, page {PageNumber}",
                    addressDtos.Count, request.UserId, request.PageNumber);

                return ApiResponse<PaginatedResponse<AddressDto>>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for user {UserId}", request.UserId);
                return ApiResponse<PaginatedResponse<AddressDto>>.Failure("An error occurred while retrieving addresses");
            }
        }
    }
}
