using IdentityService.Features.Shared;
using MediatR;

namespace IdentityService.Features.Commands.Login
{
    public static class LoginEndpoint
    {
        public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
        {
       
            app.MapPost("/api/auth/login", async (
                LoginCommand command,
                IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
           
                var userCommand = command with { RequiredRole = null };
                var result = await handler.Handle(userCommand, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(
                        EndpointResponse<LoginResponseDto>.Success(result.Data!, result.Message, result.StatusCode))
                    : Results.StatusCode(result.StatusCode);
            })
            .WithName("UserLogin")
            .WithTags("Authentication", "User")
            .Produces<EndpointResponse<LoginResponseDto>>(200)
            .Produces<EndpointResponse<LoginResponseDto>>(400)
            .Produces<EndpointResponse<LoginResponseDto>>(401)
            .Produces<EndpointResponse<LoginResponseDto>>(403)
            .Produces<EndpointResponse<LoginResponseDto>>(500);

            app.MapPost("/api/admin/auth/login", async (
                LoginCommand command,
                IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
        
                var adminCommand = command with { RequiredRole = "Admin" };
                var result = await handler.Handle(adminCommand, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(
                        EndpointResponse<LoginResponseDto>.Success(result.Data!, result.Message, result.StatusCode))
                    : Results.StatusCode(result.StatusCode);
            })
            .WithName("AdminLogin")
            .WithTags("Authentication", "Admin")
            .Produces<EndpointResponse<LoginResponseDto>>(200)
            .Produces<EndpointResponse<LoginResponseDto>>(400)
            .Produces<EndpointResponse<LoginResponseDto>>(401)
            .Produces<EndpointResponse<LoginResponseDto>>(403)
            .Produces<EndpointResponse<LoginResponseDto>>(500);


            app.MapPost("/api/auth/login/{role}", async (
                string role,
                LoginCommand command,
                IRequestHandler<LoginCommand, RequestResponse<LoginResponseDto>> handler,
                CancellationToken cancellationToken) =>
            {
                var roleSpecificCommand = command with { RequiredRole = role };
                var result = await handler.Handle(roleSpecificCommand, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(
                        EndpointResponse<LoginResponseDto>.Success(result.Data!, result.Message, result.StatusCode))
                    : Results.StatusCode(result.StatusCode);
            })
            .WithName("RoleSpecificLogin")
            .WithTags("Authentication")
            .Produces<EndpointResponse<LoginResponseDto>>(200)
            .Produces<EndpointResponse<LoginResponseDto>>(400)
            .Produces<EndpointResponse<LoginResponseDto>>(401)
            .Produces<EndpointResponse<LoginResponseDto>>(403)
            .Produces<EndpointResponse<LoginResponseDto>>(500);
           // .ExcludeFromDescription();
        }
    }
}
