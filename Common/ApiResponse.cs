using System.Text.Json.Serialization;

namespace StoreManagement.Common;

public class ApiResponse<T>
{
    public int Code { get; set; }

    public string Message { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Errors { get; set; }

    public static ApiResponse<T> Success(T? data)
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Message = "Thành công",
            Data = data
        };
    }

    public static ApiResponse<T> Success(string message, T? data)
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Error(int code, string message, Dictionary<string, string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Message = message,
            Errors = errors
        };
    }
}
