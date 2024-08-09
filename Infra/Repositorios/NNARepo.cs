using Microsoft.EntityFrameworkCore;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Request;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using Core.DTOs;
using Core.Response;


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
            List<ContactoNNA> li = new List<ContactoNNA>();

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
            List<FiltroNNA> lista = new List<FiltroNNA>();

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
                    if (results.Count() >0)
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
                        FiltroNNADto dto = new FiltroNNADto
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

        public void ActualizarNNASeguimiento(NNASeguimientoRequest request)
        {
            NNAs? nna = (from nn in _context.NNAs
                         where nn.Id == request.NNAId
                         select nn).FirstOrDefault();

            if (nna != null)
            {
                ContactoNNA? contacto;

                foreach (ContactoRequest c in request.Contactos)
                {
                    contacto = (from contact in _context.ContactoNNAs
                                where contact.Nombres == c.Nombre && contact.NNAId == request.NNAId
                                select contact).FirstOrDefault();

                    if (contacto != null)
                    {
                        contacto.ParentescoId = c.IdParentesco;
                        contacto.Telefonos = string.Concat(c.Telefono1, " ", c.Telefono2);

                        _context.ContactoNNAs.Update(contacto);
                    }
                    else
                    {
                        contacto = new ContactoNNA();
                        contacto.NNAId = request.NNAId;
                        contacto.Nombres = c.Nombre;
                        contacto.ParentescoId = c.IdParentesco;
                        contacto.Telefonos = string.Concat(c.Telefono1, " ", c.Telefono2);
                        _context.ContactoNNAs.Add(contacto);
                    }
                    _context.SaveChanges();
                }

                nna.OrigenReporteId = request.IdOrigenEstrategia;
                nna.PrimerNombre = request.PrimerNombreNNA;
                nna.SegundoNombre = request.SegundoNombreNNA;
                nna.PrimerApellido = request.PrimerApellidoNNA;
                nna.SegundoApellido = request.SegundoApellidoNNA;
                nna.TipoIdentificacionId = request.IdTipoIdentificacionNNA;
                nna.NumeroIdentificacion = request.NumeroIdentificacionNNA;
                nna.FechaNacimiento = request.FechaNacimientoNNA;
                nna.EtniaId = request.IdEtniaNNA;
                nna.GrupoPoblacionId = request.IdGrupoPoblacionalNNA;
                nna.SexoId = request.IdSexoNNA;
                nna.TipoRegimenSSId = request.IdRegimenAfiliacionNNA;
                nna.EAPBId = request.EAPBNNA;
                nna.OrigenReporteOtro = request.OtroOrigenEstrategia;
                nna.PaisId = request.IdPaisNacimientoNNA;

                _context.Update(nna);
                _context.SaveChanges();
            }
        }



        /**
         * Lista de parametros
        */
        public List<TPTipoIdentificacionDto> GetTpTipoId()
        {
            List<TPTipoIdentificacionDto> list = new List<TPTipoIdentificacionDto>
            {
                new TPTipoIdentificacionDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPTipoIdentificacionDto
                {
                    Id = 2,
                    Name = "NUI"
                },
                new TPTipoIdentificacionDto
                {
                    Id = 3,
                    Name = "NUI"
                },
                new TPTipoIdentificacionDto
                {
                    Id = 4,
                    Name = "NUI"
                },
                new TPTipoIdentificacionDto
                {
                    Id = 5,
                    Name = "NUI"
                },
                new TPTipoIdentificacionDto
                {
                    Id = 6,
                    Name = "NUI"
                }
            };
            return list;
        }

        public List<TPTipoIdentificacionDto> GetTPTipoIdentificacion()
        {
            List<TPTipoIdentificacionDto> list = new List<TPTipoIdentificacionDto>
            {
                new TPTipoIdentificacionDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPTipoIdentificacionDto
                {
                    Id = 2,
                    Name = "NUI"
                }
            };
            return list;
        }

        public List<TPRegimenAfiliacionDto> GetTPRegimenAfiliacion()
        {
            List<TPRegimenAfiliacionDto> list = new List<TPRegimenAfiliacionDto>
            {
                new TPRegimenAfiliacionDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPRegimenAfiliacionDto
                {
                    Id = 2,
                    Name = "NUI"
                }
            };
            return list;
        }

        public List<TPParentescoDto> GetTPParentesco()
        {
            List<TPParentescoDto> list = new List<TPParentescoDto>
            {
                new TPParentescoDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPParentescoDto
                {
                    Id = 2,
                    Name = "NUI"
                }
            };
            return list;
        }

        public List<TPPaisDto> GetTPPais()
        {
            List<TPPaisDto> list = new List<TPPaisDto>
            {
                new TPPaisDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPPaisDto
                {
                    Id = 2,
                    Name = "NUI"
                }
            };
            return list;
        }
        public List<TPDepartamentoDto> GetTPDepartamento(int PaisId)
        {
            List<TPDepartamentoDto> list = new List<TPDepartamentoDto>
            {
                new TPDepartamentoDto
                {
                    Id = 1,
                    Name = "TI",
                    PaisId = PaisId
                },
                new TPDepartamentoDto
                {
                    Id = 2,
                    Name = "NUI",
                    PaisId = PaisId
                }
            };
            return list;
        }

        public List<TPCiudadDto> GetTPCiudad(int DepartamentoId)
        {
            List<TPCiudadDto> list = new List<TPCiudadDto>
            {
                new TPCiudadDto
                {
                    Id = 1,
                    Name = "TI",
                    DepartamentoId = DepartamentoId
                },
                new TPCiudadDto
                {
                    Id = 2,
                    Name = "NUI",
                    DepartamentoId = DepartamentoId
                }
            };
            return list;
        }
        public List<TPOrigenReporteDto> GetTPOrigenReporte()
        {
            List<TPOrigenReporteDto> list = new List<TPOrigenReporteDto>
            {
                new TPOrigenReporteDto
                {
                    Id = 1,
                    Name = "SIVIGILA"
                },
                new TPOrigenReporteDto
                {
                    Id = 2,
                    Name = "Entidades territoriales"
                },
                new TPOrigenReporteDto
                {
                    Id = 3,
                    Name = "Otro - Cuidadores"
                }
            };
            return list;
        }

        public List<TPGrupoPoblacionalDto> GetGrupoPoblacional()
        {
            List<TPGrupoPoblacionalDto> list = new List<TPGrupoPoblacionalDto>
            {
                new TPGrupoPoblacionalDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPGrupoPoblacionalDto
                {
                    Id = 2,
                    Name = "NUI"
                }
            };
            return list;
        }

        public List<TPEtniaDto> GetTPEtnia()
        {
            List<TPEtniaDto> list = new List<TPEtniaDto>
            {
                new TPEtniaDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPEtniaDto
                {
                    Id = 2,
                    Name = "NUI"
                }
            };
            return list;
        }

        public List<TPEAPBDto> GetTPEAPB()
        {
            List<TPEAPBDto> list = new List<TPEAPBDto>
            {
                new TPEAPBDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPEAPBDto
                {
                    Id = 2,
                    Name = "NUI"
                }
            };
            return list;
        }

        public List<TPEstadoIngresoEstrategiaDto> GetTPEstadoIngresoEstrategia()
        {
            List<TPEstadoIngresoEstrategiaDto> list = new List<TPEstadoIngresoEstrategiaDto>
            {
                new TPEstadoIngresoEstrategiaDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPEstadoIngresoEstrategiaDto
                {
                    Id = 2,
                    Name = "NUI"
                }
            };
            return list;
        }
    }

}