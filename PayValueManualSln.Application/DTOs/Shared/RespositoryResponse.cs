namespace PayValueManualSln.Application.Dtos.Shared
{
    public class RepositoryResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class ApiResponse
    {
        public int StatusId { get; set; }
        public string StatusMessage { get; set; }
        public bool IsSuccessful { get; set; }
    }

    public class ExternalApiResponse<T>
    {
        public int StatusId { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }
        public object errors { get; set; }
        public T data { get; set; }
    }
}
