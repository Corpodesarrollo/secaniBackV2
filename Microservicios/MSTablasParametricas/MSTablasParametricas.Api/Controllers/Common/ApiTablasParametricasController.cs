using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;

namespace MSTablasParametricas.Api.Controllers.Common
{
    [Route("[controller]")]
    [ApiController]
    public class ApiTablasParametricasController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public ApiTablasParametricasController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Método para obtener todos los registros de una tabla
        [HttpGet("{tableName}")]
        public async Task<ActionResult> GetAll(string tableName, CancellationToken cancellationToken)
        {
            // Obtener los tipos de entidad y DTO utilizando reflection
            var (entityType, dtoType) = GetTypesFromTableName(tableName);

            if (entityType == null || dtoType == null)
            {
                return BadRequest("Invalid table name provided.");
            }

            // Crear el tipo genérico del servicio usando reflection
            var serviceType = typeof(IGenericService<,>).MakeGenericType(entityType, dtoType);

            // Obtener la instancia del servicio genérico del contenedor de inyección de dependencias
            var service = _serviceProvider.GetService(serviceType);
            if (service == null)
            {
                return NotFound($"Service for table {tableName} not found.");
            }

            // Usar reflection para invocar el método GetAllAsync
            var getAllMethod = serviceType.GetMethod("GetAllAsync");
            var task = (Task)getAllMethod.Invoke(service, new object[] { cancellationToken });
            await task.ConfigureAwait(false);

            // Obtener el resultado del Task usando reflection
            var resultProperty = task.GetType().GetProperty("Result");
            var entities = resultProperty.GetValue(task);

            return Ok(entities);
        }

        // Método para obtener un registro específico por Id
        [HttpGet("{tableName}/{id}")]
        public async Task<ActionResult> GetById(string tableName, int id, CancellationToken cancellationToken)
        {
            var (entityType, dtoType) = GetTypesFromTableName(tableName);

            if (entityType == null || dtoType == null)
            {
                return BadRequest("Invalid table name provided.");
            }

            var serviceType = typeof(IGenericService<,>).MakeGenericType(entityType, dtoType);
            var service = _serviceProvider.GetService(serviceType);
            if (service == null)
            {
                return NotFound($"Service for table {tableName} not found.");
            }

            var getByIdMethod = serviceType.GetMethod("GetByIdAsync", new[] { typeof(int), typeof(CancellationToken) });
            var task = (Task)getByIdMethod.Invoke(service, new object[] { id, cancellationToken });
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            var entity = resultProperty.GetValue(task);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        [HttpGet("NombreTablas")]
        public ActionResult GetAllTableNames()
        {
            // Obtener los nombres de las tablas del método GetTypesFromTableName
            var tableNames = new List<string>
            {
                "categoriaalerta",
                "causainasistencia",
                "cie10",
                "estadoalerta",
                "estadoingresoestrategia",
                "estadonna",
                "estadoseguimiento",
                "festivos",
                "malaatencionips",
                "motivocierresolicitud",
                "origenreporte",
                "razonessindiagnostico",
                "subcategoriaalerta",
                "tipofallallamada"
            };

            return Ok(tableNames);
        }

        [HttpGet("NombreTablasSISPRO")]
        public ActionResult GetAllTableNamesSISPRO()
        {
            // Obtener los nombres de las tablas del método GetTypesFromTableName
            var tableNames = new List<string>
            {
                "IPSCodHabilitacion",
                "Departamento",
                "Municipio",
                "ZonaTerritorial",
                "EstratoSocioeconomico",
                "RIBATipoVivienda",
                "UnidadMedida",
                "APSTipoIdentificacion",
                "APSRegimenAfiliacion",
                "CodigoEAPByNit",
                "EntidadTerritorial",
                "GrupoEtnico",
                "LCETipoPoblacionEspecial",
                "RLCPDParentesco"
            };

            return Ok(tableNames);
        }

        // Método para mapear el nombre de la tabla con los tipos correspondientes
        private (Type entityType, Type dtoType) GetTypesFromTableName(string tableName)
        {
            switch (tableName.ToLower())
            {
                case "categoriaalerta":
                    return (typeof(TPCategoriaAlerta), typeof(CategoriaAlertaDTO));
                case "causainasistencia":
                    return (typeof(TPCausaInasistencia), typeof(GenericTPDTO));
                case "cie10":
                    return (typeof(TPCIE10), typeof(CIE10DTO));
                case "estadoalerta":
                    return (typeof(TPEstadoAlerta), typeof(GenericTPDTO));
                case "estadoingresoestrategia":
                    return (typeof(TPEstadoIngresoEstrategia), typeof(GenericTPDTO));
                case "estadonna":
                    return (typeof(TPEstadoNNA), typeof(GenericTPDTO));
                case "estadoseguimiento":
                    return (typeof(TPEstadoSeguimiento), typeof(GenericTPDTO));
                case "festivos":
                    return (typeof(TPFestivos), typeof(FestivoDTO));
                case "malaatencionips":
                    return (typeof(TPMalaAtencionIPS), typeof(GenericTPDTO));
                case "motivocierresolicitud":
                    return (typeof(TPMotivoCierreSolicitud), typeof(GenericTPDTO));
                case "origenreporte":
                    return (typeof(TPOrigenReporte), typeof(GenericTPDTO));
                case "razonessindiagnostico":
                    return (typeof(TPRazonesSinDiagnostico), typeof(GenericTPDTO));
                case "subcategoriaalerta":
                    return (typeof(TPSubCategoriaAlerta), typeof(SubCategoriaAlertaDTO));
                case "tipofallallamada":
                    return (typeof(TPTipoFallaLlamada), typeof(GenericTPDTO));
                default:
                    return (null, null);
            }
        }
    }


}
