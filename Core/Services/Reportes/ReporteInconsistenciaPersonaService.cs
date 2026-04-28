using Core.DTOs;
using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Interfaces.Services.Reportes;
using Core.Modelos;
using Mapster;

namespace Core.Services.Reportes
{
    public class ReporteInconsistenciaPersonaService(IPersonaService personaService, IReporteInconsistenciaPersonaRepository repository) : IReporteInconsistenciaPersonaService
    {
        private readonly IPersonaService _personaService = personaService;
        private readonly IReporteInconsistenciaPersonaRepository _repository = repository;
        public async Task<ReporteInconsistenciaPersonaDTO> AddReporteInconsistenciaAsync(NNADto menor)
        {
            try
            {
                // Intentar obtener la identificación de la persona
                var identificacionPersona = await _personaService.GetIdVigenteAsync(menor.TipoIdentificacionId, menor.NumeroIdentificacion);
                ReporteInconsistenciaPersonaDTO reporte = null;

                if (identificacionPersona is VIdentificacionPersona persona)
                {
                    var inconsistencia = new ReporteInconsistenciaPersonaDTO
                    {
                        NNAId = menor.Id,
                        FechaReporte = DateTime.Now,
                        TipoIdentificacionSIVIGILA = persona.Tipo_identificacion,
                        TipoIdentificacion = menor.TipoIdentificacionId,
                        NumeroIdentificacionSIVIGILA = persona.Numero_identificacion,
                        NumeroIdentificacion = menor.NumeroIdentificacion,
                        PrimerNombreSIVIGILA = persona.Primer_nombre,
                        PrimerNombre = menor.PrimerNombre,
                        SegundoNombreSIVIGILA = persona.Segundo_nombre,
                        SegundoNombre = menor.SegundoNombre,
                        PrimerApellidoSIVIGILA = persona.Primer_apellido,
                        PrimerApellido = menor.PrimerApellido,
                        SegundoApellidoSIVIGILA = persona.Segundo_apellido,
                        SegundoApellido = menor.SegundoApellido,
                        FechaNacimientoSIVIGILA = persona.Fecha_nacimiento?.DateTime,
                        FechaNacimiento = menor.FechaNacimiento,
                        SexoIdSIVIGILA = persona.Sexo,
                        SexoId = menor.SexoId,
                        FechaDefuncionSIVIGILA = persona.FechaFallecimiento?.DateTime,
                        FechaDefuncion = menor.FechaDefuncion
                    };

                    // Comparaciones y determinación de inconsistencias
                    if (NormalizeString(persona.Tipo_identificacion) != NormalizeString(menor.TipoIdentificacionId))
                        inconsistencia.TipoIdentificacionInconsistente = true;

                    if (NormalizeString(persona.Numero_identificacion) != NormalizeString(menor.NumeroIdentificacion))
                        inconsistencia.NumeroIdentificacionInconsistente = true;

                    if (NormalizeString(persona.Primer_nombre) != NormalizeString(menor.PrimerNombre))
                        inconsistencia.PrimerNombreInconsistente = true;

                    if (NormalizeString(persona.Segundo_nombre) != NormalizeString(menor.SegundoNombre))
                        inconsistencia.SegundoNombreInconsistente = true;

                    if (NormalizeString(persona.Primer_apellido) != NormalizeString(menor.PrimerApellido))
                        inconsistencia.PrimerApellidoInconsistente = true;

                    if (NormalizeString(persona.Segundo_apellido) != NormalizeString(menor.SegundoApellido))
                        inconsistencia.SegundoApellidoInconsistente = true;

                    if (persona.Fecha_nacimiento?.DateTime != menor.FechaNacimiento)
                        inconsistencia.FechaNacimientoInconsistente = true;

                    if (NormalizeString(persona.Sexo) != NormalizeString(menor.SexoId))
                        inconsistencia.SexoIdInconsistente = true;

                    if (persona.FechaFallecimiento?.DateTime != menor.FechaDefuncion)
                        inconsistencia.FechaDefuncionInconsistente = true;

                    // Si hay al menos una inconsistencia, agregar al reporte
                    if (inconsistencia.TipoIdentificacionInconsistente ||
                        inconsistencia.NumeroIdentificacionInconsistente ||
                        inconsistencia.PrimerNombreInconsistente ||
                        inconsistencia.SegundoNombreInconsistente ||
                        inconsistencia.PrimerApellidoInconsistente ||
                        inconsistencia.SegundoApellidoInconsistente ||
                        inconsistencia.FechaNacimientoInconsistente ||
                        inconsistencia.SexoIdInconsistente ||
                        inconsistencia.FechaDefuncionInconsistente)
                    {
                        reporte = inconsistencia;
                    }
                }
                // BUG-LZ-004: si no hay inconsistencias o persona no encontrada, no insertar
                if (reporte == null) return null;
                var reporteNNA = reporte.Adapt<ReporteInconsistenciaPersona>();
                await _repository.AddReporteInconsistenciaAsync(reporteNNA);
                return reporte;

            }
            catch (HttpRequestException ex)
            {
                // Manejo específico para fallos en la solicitud HTTP
                Console.WriteLine($"Error de conexión con el servicio: {ex.Message}");
                throw new Exception("No se pudo conectar con el servicio de identificación de persona.", ex);
            }
            catch (Exception ex)
            {
                // Manejo general para otras excepciones
                Console.WriteLine($"Error inesperado: {ex.Message}");
                throw new Exception("Ocurrió un error al generar el reporte de inconsistencia.", ex);
            }
        }
        public async Task<ReporteInconsistenciaPersonaDTO> GetReporteInconsistenciaPersonaByIdAsync(long id)
        {
            var reporte = await _repository.GetReporteInconsistenciaPersonaByIdAsync(id);
            if (reporte == null)
                return null;
            return reporte.Adapt<ReporteInconsistenciaPersonaDTO>();
        }

        public async Task<List<ReporteInconsistenciaPersonaDTO>> GetReporteInconsistenciasPersonaAsync()
        {
            // Obtener todos los reportes desde el repositorio
            var reportes = await _repository.GetReporteInconsistenciasPersonaAsync();

            // Si no hay reportes, retornar una lista vacía
            if (reportes == null || !reportes.Any())
                return new List<ReporteInconsistenciaPersonaDTO>();

            // Adaptar la lista de reportes a una lista de DTOs
            return reportes.Adapt<List<ReporteInconsistenciaPersonaDTO>>();
        }

        private static string NormalizeString(string input)
        {
            return input?.Trim().ToUpper() ?? string.Empty;
        }

        public async Task<InconsistenciaReporte> GetReporteInconsistenciasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _repository.GetReporteInconsistenciasAsync(fechaInicio, fechaFin);
        }
    }
}
