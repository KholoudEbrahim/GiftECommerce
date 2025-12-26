namespace OrderService.Features.Shared
{
    public interface IUserContext
    {
        Guid UserId { get; }
        bool IsAuthenticated { get; }
    }
}
