namespace IdentityService.Features.Shared
{
    public record RequestResponse<T>(T Data, string Message = "", bool IsSuccess = true, int StatusCode = 200)
    {
        public static RequestResponse<T> Success(T data, string message = "", int statusCode = 200) =>
            new(data, message, true, statusCode);

        public static RequestResponse<T> Fail(string message, int statusCode) =>
            new(default!, message, false, statusCode);
    }
}
