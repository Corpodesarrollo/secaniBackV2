using Core.Common;
using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SISPRO.TRV.Web.MVCCore;
using System.Reflection;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace MSSeguimiento.Api.Controllers
{
    public class AuthController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<UserDto>> Get()
        {
            var user = this.GetUser();
            return Ok(user);
        }

        [HttpGet("export-xls")]
        public IActionResult ExportControllersAsExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Para evitar problemas con EPPlus

            // Obtener los controladores y métodos
            var controllers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ControllerBase)))
                .Select(controller => new
                {
                    Nombre = controller.Name,
                    Métodos = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(m => m.DeclaringType == controller)
                        .Select(method => new
                        {
                            Método = method.Name,
                            TipoRetorno = method.ReturnType.Name,
                            VerboHttp = GetHttpVerb(method)
                        })
                })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Controladores");

                // Encabezados
                worksheet.Cells[1, 1].Value = "Controlador";
                worksheet.Cells[1, 2].Value = "Funcion";
                worksheet.Cells[1, 3].Value = "Metodo";


                int row = 2;
                foreach (var controller in controllers)
                {
                    foreach (var metodo in controller.Métodos)
                    {
                        worksheet.Cells[row, 1].Value = controller.Nombre.Replace("Controller", "");
                        worksheet.Cells[row, 2].Value = metodo.Método;
                        worksheet.Cells[row, 3].Value = metodo.VerboHttp;
                        row++;
                    }
                }

                worksheet.Cells.AutoFitColumns(); // Ajustar columnas

                // Convertir el archivo a Base64
                byte[] fileBytes = package.GetAsByteArray();
                string base64Xls = Convert.ToBase64String(fileBytes);

                return Ok(new { Base64Xls = base64Xls });
            }
        }

        private string GetHttpVerb(MethodInfo method)
        {
            if (method.GetCustomAttributes(typeof(HttpGetAttribute), false).Any())
                return "GET";
            if (method.GetCustomAttributes(typeof(HttpPostAttribute), false).Any())
                return "POST";
            if (method.GetCustomAttributes(typeof(HttpPutAttribute), false).Any())
                return "PUT";
            if (method.GetCustomAttributes(typeof(HttpDeleteAttribute), false).Any())
                return "DELETE";
            return "UNKNOWN"; // Si no tiene atributo HTTP, lo marcamos como "UNKNOWN"
        }
    }
}
