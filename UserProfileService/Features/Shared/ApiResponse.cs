namespace UserProfileService.Features.Shared
{
  public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; }

        protected ApiResponse(bool isSuccess, T? data, List<string> errors)
        {
            IsSuccess = isSuccess;
            Data = data;
            Errors = errors;
            Timestamp = DateTime.UtcNow;
        }

        public static ApiResponse<T> Success(T data)
        {
            return new ApiResponse<T>(true, data, new List<string>());
        }

        public static ApiResponse<T> Failure(string error)
        {
            return new ApiResponse<T>(false, default, new List<string> { error });
        }

        public static ApiResponse<T> Failure(IEnumerable<string> errors)
        {
            return new ApiResponse<T>(false, default, errors.ToList());
        }
    }

    public class ApiResponse : ApiResponse<object>
    {

        private ApiResponse(bool isSuccess, object? data, List<string> errors)
            : base(isSuccess, data, errors)
        {
        }

        public static new ApiResponse Success(object data)
        {
            return new ApiResponse(true, data, new List<string>());
        }

        public static new ApiResponse Failure(string error)
        {
            return new ApiResponse(false, default, new List<string> { error });
        }
    }

}
