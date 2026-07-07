using Api.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Configurations
{
    public sealed class ValidacionFilter(IServiceProvider servicios) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argumento in context.ActionArguments.Values)
            {
                if (argumento is null) continue;

                var tipoValidador = typeof(IValidator<>).MakeGenericType(argumento.GetType());
                if (servicios.GetService(tipoValidador) is not IValidator validador) continue;

                var resultado = await validador.ValidateAsync(new ValidationContext<object>(argumento));
                if (!resultado.IsValid)
                {
                    // Tomamos el primer error para mantener el formato de un solo string que usas en el Program.cs
                    var primerError = resultado.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                                      ?? "Error de validación.";

                    // Corta la petición con tu formato estándar
                    context.Result = new BadRequestObjectResult(ApiResponse<object>.Fail(primerError));
                    return;
                }
            }

            await next();
        }
    }
}
