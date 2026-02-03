
namespace SystemBrightSpotBE.Base
{
    public class BaseResponse
    {
        public string? Message { get; set; } = string.Empty;
        public object? Data { get; set; } = null;
        public string? ErrorMessage { get; set; } = null;
        public object? ErrorDetails { get; set; } = null;

        public BaseResponse(string? message = null, object? data = null, object? errorDetails = null, string? errorMessage = null)
        {
            this.Message = message;
            this.Data = data;
            this.ErrorDetails = errorDetails;
            this.ErrorMessage = errorMessage;
        }
    }
}
