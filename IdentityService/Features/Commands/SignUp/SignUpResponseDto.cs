namespace IdentityService.Features.Commands.SignUp
{
    public record SignUpResponseDto
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;

    }
}
