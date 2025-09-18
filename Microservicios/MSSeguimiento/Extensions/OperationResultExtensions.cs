using Core.DTOs.AusenciasUsuario;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Extensions
{
    public static class OperationResultExtensions
    {
        /// <summary>
        /// Convierte un OperationResult en IActionResult siguiendo convenciones HTTP.
        /// </summary>
        public static IActionResult ToActionResult<T>(this OperationResult<T> result)
        {
            if (result is null)
                return new BadRequestObjectResult(new[] { new { Code = "NULL_RESULT", Message = "Resultado nulo." } });

            if (result.Success)
                return new OkObjectResult(result.Data);

            // Clasificación por códigos de error
            var codes = result.Errors.Select(e => (e.Code ?? string.Empty).ToUpperInvariant()).ToHashSet();

            if (codes.Contains("NO_ENCONTRADO") || codes.Contains("NOT_FOUND"))
                return new NotFoundObjectResult(result.Errors);

            if (codes.Contains("DUPLICADO") || codes.Contains("CONFLICT"))
                return new ConflictObjectResult(result.Errors);

            // Validaciones u otros
            if (codes.Contains("VALIDATION") ||
                codes.Contains("FECHA_NO_FUTURA") ||
                codes.Contains("USUARIO_REQUERIDO") ||
                codes.Contains("ARG_NULL"))
                return new BadRequestObjectResult(result.Errors);

            // Fallback
            return new BadRequestObjectResult(result.Errors);
        }
    }
}