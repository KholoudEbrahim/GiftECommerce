namespace UserProfileService.Services
{
   
    public interface IIdentityServiceClient
    {
        Task<UserIdentityDto?> GetUserIdentityAsync(Guid userId, CancellationToken cancellationToken = default);
    }

 
}
