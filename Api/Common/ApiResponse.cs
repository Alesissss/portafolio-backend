using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Api.Common
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public static ApiResponse<T> Success(T data, string message = "OK") => 
            new ApiResponse<T> { Data = data, Status = true, Message = message };

        // Sobrecarga para cuando los datos enviados son nulos, pero se quiere enviar un mensaje de éxito
        public static ApiResponse<T> Success(string message = "OK") =>
            new ApiResponse<T> { Data = default, Status = true, Message = message };

        public static ApiResponse<T> Fail(string message = "Fail") =>
            new ApiResponse<T> { Data = default, Status = false, Message = message };
    }
}
