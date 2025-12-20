using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserProfileService.Features.Shared
{
    public static class ApiResponseExtensions
    {
        public static IResult ToResult<T>(this ApiResponse<T> response)
        {
            return response.IsSuccess
                ? Results.Ok(response)
                : Results.BadRequest(response);
        }

        public static WebApplicationBuilder AddApiResponseConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            return builder;
        }

    }
}
