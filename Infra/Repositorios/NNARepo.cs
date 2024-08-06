using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.Response;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace Infra.Repositorios
{
    public class NNARepo : INNARepo
    {
        private readonly ApplicationDbContext _context;

        public NNARepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public RespuestaResponse<ContactoNNA> CrearContactoNNA(ContactoNNA contactoNNA)
        {
            var response = new RespuestaResponse<ContactoNNA>();

            try
            {
                _context.Set<ContactoNNA>().Add(contactoNNA);
                _context.SaveChangesAsync();
                response.Estado = true;
                response.Descripcion = "Contacto creado con éxito.";
                response.Datos = null;  // Aquí puedes devolver el objeto creado si es necesario.
            }
            catch (Exception ex)
            {
                response.Estado = false;
                response.Descripcion = $"Error al crear el contacto: {ex.Message}";
                response.Datos = null;
            }

            return response;
        }

        public RespuestaResponse<ContactoNNA> ActualizarContactoNNA(ContactoNNA contactoNNA)
        {
            var response = new RespuestaResponse<ContactoNNA>();

            try
            {
                _context.Set<ContactoNNA>().Update(contactoNNA);
                _context.SaveChangesAsync();
                response.Estado = true;
                response.Descripcion = "Contacto actualizado con éxito.";
                response.Datos = null;  // Aquí puedes devolver el objeto creado si es necesario.
            }
            catch (Exception ex)
            {
                response.Estado = false;
                response.Descripcion = $"Error al actualizar el contacto: {ex.Message}";
                response.Datos = null;
            }

            return response;
        }

        public List<TPEstadoNNA> TpEstadosNNA()
        {
            var response = _context.TPEstadoNNA
                                    .Select(e => new TPEstadoNNA
                                    {
                                        Id = e.Id,
                                        Nombre = e.Nombre,
                                        Descripcion = e.Descripcion,
                                        ColorBG = e.ColorBG,
                                        ColorText = e.ColorText
                                    })
                                    .ToList();

            return response;
        }

        public List<VwAgentesAsignados> VwAgentesAsignados()
        {
            var response = _context.VwAgentesAsignados.ToList();

            return response;
        }

        public RespuestaResponse<ContactoNNA> ObtenerContactoPorId(long NNAId)
        {
            var response = new RespuestaResponse<ContactoNNA>();
            List<ContactoNNA> li = new();

            try
            {
                var contacto = _context.ContactoNNAs.Find(NNAId);

                if (contacto != null)
                {
                    response.Estado = true;
                    response.Descripcion = "Contacto obtenido con éxito.";
                    li.Add(contacto);
                    response.Datos = li;
                }
                else
                {
                    response.Estado = false;
                    response.Descripcion = "No se encontró el contacto con el ID especificado.";
                    response.Datos = null;
                }
            }
            catch (Exception ex)
            {
                response.Estado = false;
                response.Descripcion = $"Error al obtener el contacto: {ex.Message}";
                response.Datos = null;
            }

            return response;
        }

        public NNAs ConsultarNNAsByTipoIdNumeroId(int tipoIdentificacionId, string numeroIdentificacion)
        {
            var response = new NNAs();

            try
            {
                var nnna = _context.NNAs.FirstOrDefault(x => x.TipoIdentificacionId == tipoIdentificacionId && x.NumeroIdentificacion==numeroIdentificacion);

                if (nnna != null)
                {
                    response = nnna;
                }
                else
                {
                    response = null;
                }
            }
            catch (Exception ex)
            {
                response = null;
            }

            return response;
        }

        public RespuestaResponse<FiltroNNADto> ConsultarNNAFiltro(FiltroNNARequest entrada)
        {
            var response = new RespuestaResponse<FiltroNNADto>();
            response.Datos = new List<FiltroNNADto>();
            List<FiltroNNA> lista = new();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Estado", entrada.Estado ?? 0),
                    new SqlParameter("@Agente", entrada.Agente ?? ""),
                    new SqlParameter("@Buscar", entrada.Buscar ?? ""),
                    new SqlParameter("@Orden", entrada.Orden ?? 1)
                };

                var results = _context.FiltroNNAs.FromSqlRaw(
                    "EXEC dbo.SpConsultaNnaFiltro @Estado, @Agente, @Buscar, @Orden",
                    parameters
                ).ToList();
                if (results != null)
                {
                    if (results.Count() > 0)
                    {
                        response.Estado = true;
                        response.Descripcion = "Consulta realizada con éxito.";
                    }
                    else
                    {
                        response.Estado = false;
                        response.Descripcion = "No trajo datos en la consulta.";
                    }

                    foreach (var filtroNNA in results)
                    {
                        FiltroNNADto dto = new()
                        {
                            NoCaso = filtroNNA.NoCaso,
                            NombreNNA = filtroNNA.NombreNNA,
                            NoDocumento = filtroNNA.NoDocumento,
                            UltimaActualizacion = filtroNNA.UltimaActualizacion,
                            AgenteAsignado = filtroNNA.AgenteAsignado,
                            EstadoId = filtroNNA.EstadoId,
                            Estado = filtroNNA.Estado,
                            EstadoDescripcion = filtroNNA.EstadoDescripcion,
                            EstadoColorBG = filtroNNA.EstadoColorBG,
                            EstadoColorText = filtroNNA.EstadoColorText
                        };

                        response.Datos.Add(dto);
                    }
                }
                else
                {
                    response.Estado = true;
                    response.Descripcion = "No trae información.";
                    response.Datos = null;
                }

            }
            catch (Exception ex)
            {
                response.Estado = false;
                response.Descripcion = $"Error al realizar la consulta: {ex.Message}";
                response.Datos = null;
            }

            return response;
        }
    }

}