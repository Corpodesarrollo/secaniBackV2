using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
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

        private IQueryable<NNADto> SelectBase()
        {
            var result = from n in _context.NNAs
                         select new NNADto
                         {
                             Id = n.Id,
                             estadoId = n.estadoId,
                             ResidenciaActualCategoriaId = n.ResidenciaActualCategoriaId,
                             ResidenciaActualMunicipioId = n.ResidenciaActualMunicipioId,
                             ResidenciaActualBarrio = n.ResidenciaActualBarrio,
                             ResidenciaActualAreaId = n.ResidenciaActualAreaId,
                             ResidenciaActualDireccion = n.ResidenciaActualDireccion,
                             ResidenciaActualEstratoId = n.ResidenciaActualEstratoId,
                             ResidenciaActualTelefono = n.ResidenciaActualTelefono,
                             ResidenciaOrigenCategoriaId = n.ResidenciaOrigenCategoriaId,
                             ResidenciaOrigenMunicipioId = n.ResidenciaOrigenMunicipioId,
                             ResidenciaOrigenBarrio = n.ResidenciaOrigenBarrio,
                             ResidenciaOrigenAreaId = n.ResidenciaOrigenAreaId,
                             ResidenciaOrigenDireccion = n.ResidenciaOrigenDireccion,
                             ResidenciaOrigenEstratoId = n.ResidenciaOrigenEstratoId,
                             ResidenciaOrigenTelefono = n.ResidenciaOrigenTelefono,
                             FechaNotificacionSIVIGILA = n.FechaNotificacionSIVIGILA,
                             PrimerNombre = n.PrimerNombre,
                             SegundoNombre = n.SegundoNombre,
                             PrimerApellido = n.PrimerApellido,
                             SegundoApellido = n.SegundoApellido,
                             TipoIdentificacionId = n.TipoIdentificacionId,
                             NumeroIdentificacion = n.NumeroIdentificacion,
                             FechaNacimiento = n.FechaNacimiento,
                             MunicipioNacimientoId = n.MunicipioNacimientoId,
                             SexoId = n.SexoId,
                             TipoRegimenSSId = n.TipoRegimenSSId,
                             EAPBId = n.EAPBId,
                             EPSId = n.EPSId,
                             IPSId = n.IPSId,
                             GrupoPoblacionId = n.GrupoPoblacionId,
                             EtniaId = n.EtniaId,
                             EstadoIngresoEstrategiaId = n.EstadoIngresoEstrategiaId,
                             FechaIngresoEstrategia = n.FechaIngresoEstrategia,
                             OrigenReporteId = n.OrigenReporteId,
                             FechaConsultaOrigenReporte = n.FechaConsultaOrigenReporte,
                             TipoCancerId = n.TipoCancerId,
                             FechaInicioSintomas = n.FechaInicioSintomas,
                             FechaHospitalizacion = n.FechaHospitalizacion,
                             FechaDefuncion = n.FechaDefuncion,
                             MotivoDefuncion = n.MotivoDefuncion,
                             FechaInicioTratamiento = n.FechaInicioTratamiento,
                             Recaida = n.Recaida,
                             CantidadRecaidas = n.CantidadRecaidas,
                             FechaUltimaRecaida = n.FechaUltimaRecaida,
                             TipoDiagnosticoId = n.TipoDiagnosticoId,
                             DiagnosticoId = n.DiagnosticoId,
                             FechaDiagnostico = n.FechaDiagnostico,
                             MotivoNoDiagnosticoId = n.MotivoNoDiagnosticoId,
                             MotivoNoDiagnosticoOtro = n.MotivoNoDiagnosticoOtro,
                             FechaConsultaDiagnostico = n.FechaConsultaDiagnostico,
                             DepartamentoTratamientoId = n.DepartamentoTratamientoId,
                             IPSIdTratamiento = n.IPSIdTratamiento,
                             PropietarioResidenciaActual = n.PropietarioResidenciaActual,
                             PropietarioResidenciaActualOtro = n.PropietarioResidenciaActualOtro,
                             TrasladoTieneCapacidadEconomica = n.TrasladoTieneCapacidadEconomica,
                             TrasladoEAPBSuministroApoyo = n.TrasladoEAPBSuministroApoyo,
                             TrasladosServiciosdeApoyoOportunos = n.TrasladosServiciosdeApoyoOportunos,
                             TrasladosServiciosdeApoyoCobertura = n.TrasladosServiciosdeApoyoCobertura,
                             TrasladosHaSolicitadoApoyoFundacion = n.TrasladosHaSolicitadoApoyoFundacion,
                             TrasladosNombreFundacion = n.TrasladosNombreFundacion,
                             TrasladosApoyoRecibidoxFundacion = n.TrasladosApoyoRecibidoxFundacion,
                             DifAutorizaciondeMedicamentos = n.DifAutorizaciondeMedicamentos,
                             DifEntregaMedicamentosLAP = n.DifEntregaMedicamentosLAP,
                             DifEntregaMedicamentosNoLAP = n.DifEntregaMedicamentosNoLAP,
                             DifAsignaciondeCitas = n.DifAsignaciondeCitas,
                             DifHanCobradoCuotasoCopagos = n.DifHanCobradoCuotasoCopagos,
                             DifAutorizacionProcedimientos = n.DifAutorizacionProcedimientos,
                             DifRemisionInstitucionesEspecializadas = n.DifRemisionInstitucionesEspecializadas,
                             DifMalaAtencionIPS = n.DifMalaAtencionIPS,
                             DifMalaAtencionNombreIPSId = n.DifMalaAtencionNombreIPSId,
                             DifFallasenMIPRES = n.DifFallasenMIPRES,
                             DifFallaConvenioEAPBeIPSTratante = n.DifFallaConvenioEAPBeIPSTratante,
                             CategoriaAlertaId = n.CategoriaAlertaId,
                             SubcategoriaAlertaId = n.SubcategoriaAlertaId,
                             TrasladosHaSidoTrasladadodeInstitucion = n.TrasladosHaSidoTrasladadodeInstitucion,
                             TrasladosNumerodeTraslados = n.TrasladosNumerodeTraslados,
                             TrasladosIPSId = n.TrasladosIPSId,
                             TrasladosHaRecurridoAccionLegal = n.TrasladosHaRecurridoAccionLegal,
                             TrasladosTipoAccionLegalId = n.TrasladosTipoAccionLegalId,
                             TratamientoHaDejadodeAsistir = n.TratamientoHaDejadodeAsistir,
                             TratamientoCuantoTiemposinAsistir = n.TratamientoCuantoTiemposinAsistir,
                             TratamientoUnidadMedidaIdTiempoId = n.TratamientoUnidadMedidaIdTiempoId,
                             TratamientoCausasInasistenciaId = n.TratamientoCausasInasistenciaId,
                             TratamientoCausasInasistenciaOtra = n.TratamientoCausasInasistenciaOtra,
                             TratamientoEstudiaActualmente = n.TratamientoEstudiaActualmente,
                             TratamientoHaDejadodeAsistirColegio = n.TratamientoHaDejadodeAsistirColegio,
                             TratamientoTiempoInasistenciaColegio = n.TratamientoTiempoInasistenciaColegio,
                             TratamientoTiempoInasistenciaUnidadMedidaId = n.TratamientoTiempoInasistenciaUnidadMedidaId,
                             TratamientoHaSidoInformadoClaramente = n.TratamientoHaSidoInformadoClaramente,
                             TratamientoObservaciones = n.TratamientoObservaciones,
                             CuidadorNombres = n.CuidadorNombres,
                             CuidadorParentescoId = n.CuidadorParentescoId,
                             CuidadorEmail = n.CuidadorEmail,
                             CuidadorTelefono = n.CuidadorTelefono,
                             SeguimientoLoDesea = n.SeguimientoLoDesea,
                             SeguimientoMotivoNoLoDesea = n.SeguimientoMotivoNoLoDesea,
                             OrigenReporteOtro = n.OrigenReporteOtro,
                             PaisId = n.PaisId,
                             TrasladosMotivoAccionLegal = n.TrasladosMotivoAccionLegal,
                             TrasladosPropietarioResidenciaActualId = n.TrasladosPropietarioResidenciaActualId,
                             TrasladosPropietarioResidenciaActualOtro = n.TrasladosPropietarioResidenciaActualOtro,
                             TrasladosQuienAsumioCostosTraslado = n.TrasladosQuienAsumioCostosTraslado,
                             TrasladosQuienAsumioCostosVivienda = n.TrasladosQuienAsumioCostosVivienda,
                             DateCreated = n.DateCreated,
                             DateUpdated = n.DateUpdated,
                             DateDeleted = n.DateDeleted,
                             CreatedByUserId = n.CreatedByUserId,
                             UpdatedByUserId = n.UpdatedByUserId,
                             IsDeleted = n.IsDeleted
                         };

            return result;
        }

        public async Task<NNADto?> GetById(long id)
        {
            try
            {
                var result = await SelectBase().FirstOrDefaultAsync(x => x.Id == id);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
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

        public NNAs ConsultarNNAsByTipoIdNumeroId(string tipoIdentificacionId, string numeroIdentificacion)
        {
            var response = new NNAs();

            try
            {
                var nnna = _context.NNAs.FirstOrDefault(x => x.TipoIdentificacionId == tipoIdentificacionId && x.NumeroIdentificacion == numeroIdentificacion);

                if (nnna != null)
                {
                    response = nnna;
                }
                else
                {
                    response = null;
                }
            }
            catch (Exception)
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
            List<TPTipoIdentificacionDto> list = new()
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
            List<TPTipoIdentificacionDto> list = new()
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
            List<TPRegimenAfiliacionDto> list = new()
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
            List<TPParentescoDto> list = new()
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
            List<TPPaisDto> list = new()
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
            List<TPDepartamentoDto> list = new()
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
            List<TPCiudadDto> list = new()
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
            List<TPOrigenReporteDto> list = new()
            {
                new TPOrigenReporteDto
                {
                    Id = 1,
                    Name = "TI"
                },
                new TPOrigenReporteDto
                {
                    Id = 2,
                    Name = "NUI"
                }
            };
            return list;
        }

        public List<TPGrupoPoblacionalDto> GetGrupoPoblacional()
        {
            List<TPGrupoPoblacionalDto> list = new()
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
            List<TPEtniaDto> list = new()
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
            List<TPEAPBDto> list = new()
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
            List<TPEstadoIngresoEstrategiaDto> list = new()
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

        public NNAs ConsultarNNAsById(long NNAId)
        {
            var response = new NNAs();

            try
            {
                var nnna = (from nna in _context.NNAs
                            where nna.Id == NNAId
                            select nna).FirstOrDefault();

                if (nnna != null)
                {
                    response = nnna;
                }
                else
                {
                    response = null;
                }
            }
            catch (Exception)
            {
                response = null;
            }

            return response;
        }

        public async Task<DatosBasicosNNAResponse>? ConsultarDatosBasicosNNAById(long NNAId, TablaParametricaService tablaParametricaService)
        {

            Seguimiento? seguimiento = await (from seg in _context.Seguimientos
                                              where seg.NNAId == NNAId
                                              orderby seg.Id descending
                                              select seg).FirstOrDefaultAsync();

            DatosBasicosNNAResponse? response = await (from nna in _context.NNAs
                                                       where nna.Id == NNAId
                                                       select new DatosBasicosNNAResponse()
                                                       {
                                                           Diagnostico = "",
                                                           FechaInicioSegumiento = seguimiento.FechaSeguimiento,
                                                           FechaNacimiento = nna.FechaNacimiento,
                                                           NombreCompleto = string.Join("", nna.PrimerNombre, " ", nna.SegundoNombre, " ", nna.PrimerApellido, " ", nna.SegundoApellido),
                                                           DiagnosticoId = nna.DiagnosticoId
                                                       }).FirstOrDefaultAsync();

            List<TPExternalEntityBase> cie10 = await tablaParametricaService.GetBynomTREFCodigo("CIE10", response.DiagnosticoId, CancellationToken.None);

            if (cie10 != null && cie10.Count > 0)
            {
                response.Diagnostico = cie10[0].Nombre;
            }

            return response;
        }

        public async Task<SolicitudSeguimientoCuidadorResponse> SolicitudSeguimientoCuidador(long NNAId, TablaParametricaService tablaParametricaService)
        {
            Seguimiento? seguimiento = await (from seg in _context.Seguimientos
                                              where seg.NNAId == NNAId
                                              orderby seg.Id descending
                                              select seg).FirstOrDefaultAsync();

            SolicitudSeguimientoCuidadorResponse? response = await (from nna in _context.NNAs
                                                                    where nna.Id == NNAId
                                                                    select new SolicitudSeguimientoCuidadorResponse()
                                                                    {
                                                                        Base64Adjunto = "",
                                                                        CorreoSolicitante = "",
                                                                        DiagnosticoNNA = "",
                                                                        FechaDiagnostico = nna.FechaDiagnostico,
                                                                        FechaNacimientoNNA = nna.FechaNacimiento,
                                                                        NoCaso = nna.Id,
                                                                        NombreAdjunto = "",
                                                                        NombreCompletoNNA = string.Join("", nna.PrimerNombre, " ", nna.SegundoNombre, " ", nna.PrimerApellido, " ", nna.SegundoApellido),
                                                                        NombreSolicitante = "",
                                                                        ObservacionSolicitante = seguimiento == null ? "" : seguimiento.ObservacionesSolicitante,
                                                                        SexoNNa = nna.SexoId.ToString(),
                                                                        TelefonoSolicitante = "",
                                                                        DiagnosticoId = nna.DiagnosticoId

                                                                    }).FirstOrDefaultAsync();

            List<TPExternalEntityBase> cie10 = await tablaParametricaService.GetBynomTREFCodigo("CIE10", response.DiagnosticoId, CancellationToken.None);

            if (cie10 != null && cie10.Count > 0)
            {
                response.DiagnosticoNNA = cie10[0].Nombre;
            }

            return response;
        }
    }

}