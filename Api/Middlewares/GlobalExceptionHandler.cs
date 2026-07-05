using Api.Common;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Api.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);

            httpContext.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            string mensajeError = "Ocurrió un error interno en el servidor.";

            if (exception is JsonException || exception is BadHttpRequestException)
            {
                statusCode = HttpStatusCode.BadRequest;
                mensajeError = "El formato del JSON enviado no es válido o faltan campos obligatorios.";
            }

            else if (exception is FluentValidation.ValidationException validationExc)
            {
                statusCode = HttpStatusCode.BadRequest;
                mensajeError = string.Join(" | ", validationExc.Errors.Select(e => e.ErrorMessage));
            }

            httpContext.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.Fail(mensajeError);

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
