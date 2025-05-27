using ClosedXML.Excel;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Modelos.TablasParametricas;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Infra.Repositories.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SISPRO.TRV.Entity;


namespace Infra.Repositorios
{
    public class NNARepo : INNARepo
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<NNAs> _repository;
        private readonly GenericRepository<TPCIE10> _repositoryCie10;
        private readonly INotificacionRepo _notificacionRepo;
        private readonly ISeguimientoRepo _seguimientoRepo;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly User _user;

        public NNARepo(
            ApplicationDbContext context,
            ISeguimientoRepo seguimientoRepo,
            GenericRepository<NNAs> repository,
            GenericRepository<TPCIE10> repositoryCie10,
            INotificacionRepo notificacionRepo,
            ICurrentUserProvider currentUserProvider
            )
        {
            _context = context;
            _seguimientoRepo = seguimientoRepo;
            _repository = repository;
            _repositoryCie10 = repositoryCie10;
            _notificacionRepo = notificacionRepo;
            _currentUserProvider = currentUserProvider;
            _user = _currentUserProvider.CurrentUser;
        }


        public IQueryable<NNADto> SelectBase()
        {
            try
            {

                return from nna in _context.NNAs

                       join ips in _context.TPIPS on nna.IPSId equals ips.Id into ipsJoin
                       from ips in ipsJoin.DefaultIfEmpty()

                       join eps in _context.TPIPS on nna.EPSId equals eps.Id into epsJoin
                       from eps in epsJoin.DefaultIfEmpty()

                       join eapb in _context.TPEAPB on nna.EAPBId equals eapb.Id into eapbJoin
                       from eapb in eapbJoin.DefaultIfEmpty()

                       join p in _context.TPParentescos on nna.CuidadorParentescoId equals p.Id into pJoin
                       from p in pJoin.DefaultIfEmpty()

                       select new NNADto
                       {
                           Id = nna.Id,
                           PrimerNombre = nna.PrimerNombre,
                           SegundoNombre = nna.SegundoNombre,
                           PrimerApellido = nna.PrimerApellido,
                           SegundoApellido = nna.SegundoApellido,
                           TipoIdentificacionId = nna.TipoIdentificacionId,
                           NumeroIdentificacion = nna.NumeroIdentificacion,
                           FechaNacimiento = nna.FechaNacimiento,
                           EtniaId = nna.EtniaId,
                           GrupoPoblacionId = nna.GrupoPoblacionId,
                           SexoId = nna.SexoId,
                           TipoRegimenSSId = nna.TipoRegimenSSId,
                           EAPBId = nna.EAPBId,
                           EAPBNombre = eapb != null ? eapb.Nombre : "",
                           EPSId = nna.EPSId,
                           EPSNombre = eps != null ? eps.Nombre : "",
                           IPSId = nna.IPSId,
                           IPSNombre = ips != null ? ips.Nombre : "",
                           OrigenReporteId = nna.OrigenReporteId,
                           OrigenReporteOtro = nna.OrigenReporteOtro,
                           PaisId = nna.PaisId,
                           DiagnosticoId = nna.DiagnosticoId,
                           FechaDiagnostico = nna.FechaDiagnostico,
                           FechaInicioSintomas = nna.FechaInicioSintomas,
                           FechaHospitalizacion = nna.FechaHospitalizacion,
                           FechaDefuncion = nna.FechaDefuncion,
                           MotivoDefuncion = nna.MotivoDefuncion,
                           FechaInicioTratamiento = nna.FechaInicioTratamiento,
                           Recaida = nna.Recaida,
                           CantidadRecaidas = nna.CantidadRecaidas,
                           FechaUltimaRecaida = nna.FechaUltimaRecaida,
                           TipoDiagnosticoId = nna.TipoDiagnosticoId,
                           MotivoNoDiagnosticoId = nna.MotivoNoDiagnosticoId,
                           MotivoNoDiagnosticoOtro = nna.MotivoNoDiagnosticoOtro,
                           DepartamentoTratamientoId = nna.DepartamentoTratamientoId,
                           IPSIdTratamiento = nna.IPSIdTratamiento,
                           PropietarioResidenciaActual = nna.PropietarioResidenciaActual,
                           EstadoIngresoEstrategiaId = nna.EstadoIngresoEstrategiaId,
                           FechaIngresoEstrategia = nna.FechaIngresoEstrategia,
                           FechaConsultaOrigenReporte = nna.FechaConsultaOrigenReporte,
                           FechaNotificacionSIVIGILA = nna.FechaNotificacionSIVIGILA,
                           ResidenciaActualCategoriaId = nna.ResidenciaActualCategoriaId,
                           ResidenciaActualMunicipioId = nna.ResidenciaActualMunicipioId,
                           ResidenciaActualBarrio = nna.ResidenciaActualBarrio,
                           ResidenciaActualAreaId = nna.ResidenciaActualAreaId,
                           ResidenciaActualDireccion = nna.ResidenciaActualDireccion,
                           ResidenciaActualEstratoId = nna.ResidenciaActualEstratoId,
                           ResidenciaActualTelefono = nna.ResidenciaActualTelefono,
                           ResidenciaOrigenCategoriaId = nna.ResidenciaOrigenCategoriaId,
                           ResidenciaOrigenMunicipioId = nna.ResidenciaOrigenMunicipioId,
                           ResidenciaOrigenBarrio = nna.ResidenciaOrigenBarrio,
                           ResidenciaOrigenAreaId = nna.ResidenciaOrigenAreaId,
                           ResidenciaOrigenDireccion = nna.ResidenciaOrigenDireccion,
                           ResidenciaOrigenEstratoId = nna.ResidenciaOrigenEstratoId,
                           ResidenciaOrigenTelefono = nna.ResidenciaOrigenTelefono,
                           Contactos = (from c in _context.ContactoNNAs
                                        where c.NNAId == nna.Id
                                        select new ContactoNNADto
                                        {
                                            Id = c.Id,
                                            NNAId = c.NNAId,
                                            Nombres = c.Nombres,
                                            ParentescoId = c.ParentescoId,
                                            Telefonos = c.Telefonos,
                                            Estado = c.Estado,
                                        }).ToArray(),
                           CuidadorParentescoId = nna.CuidadorParentescoId,
                           CuidadorParentesco = p != null ? p.Nombre : "",
                           CuidadorNombres = nna.CuidadorNombres,
                           CuidadorTelefono = nna.CuidadorTelefono,
                           CuidadorEmail = nna.CuidadorEmail,
                           CategoriaAlertaId = nna.CategoriaAlertaId,
                           DifAsignaciondeCitas = nna.DifAsignaciondeCitas,
                           DifAutorizaciondeMedicamentos = nna.DifAutorizaciondeMedicamentos,
                           DifAutorizacionProcedimientos = nna.DifAutorizacionProcedimientos,
                           DifMalaAtencionIPS = nna.DifMalaAtencionIPS,
                           DifMalaAtencionNombreIPSId = nna.DifMalaAtencionNombreIPSId,
                           PropietarioResidenciaActualOtro = nna.PropietarioResidenciaActualOtro,
                           SubcategoriaAlertaId = nna.SubcategoriaAlertaId,
                           TrasladoEAPBSuministroApoyo = nna.TrasladoEAPBSuministroApoyo,
                           TrasladosApoyoRecibidoxFundacion = nna.TrasladosApoyoRecibidoxFundacion,
                           TrasladosHaRecurridoAccionLegal = nna.TrasladosHaRecurridoAccionLegal,
                           TrasladosHaSolicitadoApoyoFundacion = nna.TrasladosHaSolicitadoApoyoFundacion,
                           TrasladosMotivoAccionLegal = nna.TrasladosMotivoAccionLegal,
                           TrasladosPropietarioResidenciaActualId = nna.TrasladosPropietarioResidenciaActualId,
                           TrasladosServiciosdeApoyoCobertura = nna.TrasladosServiciosdeApoyoCobertura,
                           TrasladosServiciosdeApoyoOportunos = nna.TrasladosServiciosdeApoyoOportunos,
                           TrasladosPropietarioResidenciaActualOtro = nna.TrasladosPropietarioResidenciaActualOtro,
                           TrasladosQuienAsumioCostosTraslado = nna.TrasladosQuienAsumioCostosTraslado,
                           TrasladosQuienAsumioCostosVivienda = nna.TrasladosQuienAsumioCostosVivienda,
                           TrasladosTipoAccionLegalId = nna.TrasladosTipoAccionLegalId,
                           TratamientoCuantoTiemposinAsistir = nna.TratamientoCuantoTiemposinAsistir,
                           TratamientoEstudiaActualmente = nna.TratamientoEstudiaActualmente,
                           TratamientoHaDejadodeAsistir = nna.TratamientoHaDejadodeAsistir,
                           TratamientoHaDejadodeAsistirColegio = nna.TratamientoHaDejadodeAsistirColegio,
                           DifEntregaMedicamentosLAP = nna.DifEntregaMedicamentosLAP,
                           DifEntregaMedicamentosNoLAP = nna.DifEntregaMedicamentosNoLAP,
                           DifFallaConvenioEAPBeIPSTratante = nna.DifFallaConvenioEAPBeIPSTratante,
                           DifFallasenMIPRES = nna.DifFallasenMIPRES,
                           DifHanCobradoCuotasoCopagos = nna.DifHanCobradoCuotasoCopagos,
                           DifRemisionInstitucionesEspecializadas = nna.DifRemisionInstitucionesEspecializadas,
                           TrasladosHaSidoTrasladadodeInstitucion = nna.TrasladosHaSidoTrasladadodeInstitucion,
                           TrasladosNumerodeTraslados = nna.TrasladosNumerodeTraslados,
                           TrasladosIPSId = nna.TrasladosIPSId,
                           TratamientoCausasInasistenciaId = nna.TratamientoCausasInasistenciaId,
                           TratamientoCausasInasistenciaOtra = nna.TratamientoCausasInasistenciaOtra,
                           TratamientoHaSidoInformadoClaramente = nna.TratamientoHaSidoInformadoClaramente,
                           TratamientoObservaciones = nna.TratamientoObservaciones,
                           TratamientoTiempoInasistenciaColegio = nna.TratamientoTiempoInasistenciaColegio,
                           TratamientoTiempoInasistenciaUnidadMedidaId = nna.TratamientoTiempoInasistenciaUnidadMedidaId,
                           TratamientoUnidadMedidaIdTiempoId = nna.TratamientoUnidadMedidaIdTiempoId,
                           SeguimientoLoDesea = nna.SeguimientoLoDesea,
                           SeguimientoMotivoNoLoDesea = nna.SeguimientoMotivoNoLoDesea,
                           estadoId = nna.estadoId,
                           FechaConsultaDiagnostico = nna.FechaConsultaDiagnostico,
                           MunicipioNacimientoId = nna.MunicipioNacimientoId,
                           TipoCancerId = nna.TipoCancerId,
                           TrasladosNombreFundacion = nna.TrasladosNombreFundacion,
                           TrasladoTieneCapacidadEconomica = nna.TrasladoTieneCapacidadEconomica,
                           TratamientoRequirioCambiodeCiudad = nna.TratamientoRequirioCambiodeCiudad,
                           CreatedByUserId = nna.CreatedByUserId,
                           DateCreated = nna.DateCreated,
                           DateDeleted = nna.DateDeleted,
                           DeletedByUserId = nna.DeletedByUserId,
                           IsDeleted = nna.IsDeleted,
                           UpdatedByUserId = nna.UpdatedByUserId,
                           DateUpdated = nna.DateUpdated
                       };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<NNADto>().AsQueryable();
            }
        }

        public async Task<NNADto?> GetById(long id)
        {
            try
            {
                var nna = await SelectBase().FirstOrDefaultAsync(x => x.Id == id);
                return nna;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<(bool, NNAs)> AddAsync(NNAs entity)
        {
            var (success, response) = await _repository.AddAsync(entity);
            if (!success)
            {
                throw new KeyNotFoundException("cannot add entity");
            }
            return (success, response);
        }

        public async Task<(bool, NNAs)> UpdateAsync(NNAs entity)
        {
            try
            {
                var (success, response) = await _repository.UpdateAsync(entity);
                if (!success)
                {
                    throw new KeyNotFoundException("cannot update entity");
                }
                return (success, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        public List<VwAgentesAsignados> VwAgentesAsignados()
        {
            var response = _context.VwAgentesAsignados.ToList();

            return response;
        }

        public NNADto? ConsultarNNAsByTipoIdNumeroId(string tipoIdentificacionId, string numeroIdentificacion)
        {
            try
            {
                var result = SelectBase().FirstOrDefault(x => x.TipoIdentificacionId == tipoIdentificacionId && x.NumeroIdentificacion == numeroIdentificacion);

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public RespuestaResponse<List<FiltroNNADto>> ConsultarNNAFiltro(FiltroNNARequest entrada)
        {
            try
            {
                var query = from s in _context.Seguimientos
                            join n in _context.NNAs on s.NNAId equals n.Id
                            group s by s.NNAId into g
                            select new { id = g.Max(x => x.Id) };

                var queryResult = from q in query
                                  join s in _context.Seguimientos on q.id equals s.Id
                                  join n in _context.NNAs on s.NNAId equals n.Id
                                  join e in _context.TPEstadoNNA on n.estadoId equals e.Id

                                  join au in _context.UsuarioAsignados on s.Id equals au.SeguimientoId into auJoin
                                  from au in auJoin.DefaultIfEmpty()

                                  join a in _context.Users on au.UsuarioId equals a.Id into aJoin
                                  from a in aJoin.DefaultIfEmpty()

                                  select new SeguimientoDto()
                                  {
                                      Id = s.Id,
                                      NoCaso = s.NNAId,
                                      PrimerNombre = n.PrimerNombre,
                                      SegundoNombre = n.SegundoNombre,
                                      PrimerApellido = n.PrimerApellido,
                                      SegundoApellido = n.SegundoApellido,
                                      NumeroIdentificacion = n.NumeroIdentificacion,
                                      FechaNotificacion = n.FechaNotificacionSIVIGILA,
                                      FechaSeguimiento = s.FechaSeguimiento,
                                      Estado = new TPEstadoNNADto()
                                      {
                                          Id = e.Id,
                                          Nombre = e.Nombre,
                                          Descripcion = e.Descripcion,
                                          ColorBG = e.ColorBG,
                                          ColorText = e.ColorText
                                      },
                                      AsuntoUltimaActuacion = s.UltimaActuacionAsunto,
                                      FechaUltimaActuacion = s.UltimaActuacionFecha,
                                      UsuarioId = a.Id,
                                      Usuario = a != null ? a.FullName : "",
                                      Alertas = (from als in _context.AlertaSeguimientos
                                                 join al in _context.Alertas on als.AlertaId equals al.Id
                                                 join ea in _context.TPEstadoAlerta on als.EstadoId equals ea.Id
                                                 join sca in _context.TPSubCategoriaAlerta on al.SubcategoriaId equals sca.Id
                                                 where als.SeguimientoId == s.Id
                                                 select new Core.DTOs.AlertaSeguimientoDto { Nombre = sca.CategoriaAlertaId + "." + sca.Indicador, Id = ea.Id }).ToList()
                                  };

                IQueryable<SeguimientoDto> queryFiltro = queryResult;
                if ((entrada.Estado ?? 0) > 0)
                    queryFiltro = queryResult.Where(x => x.Estado.Id == entrada.Estado);

                if (!string.IsNullOrEmpty(entrada.Agente))
                    queryFiltro = queryFiltro.Where(x => x.UsuarioId == entrada.Agente);

                if (!string.IsNullOrEmpty(entrada.Buscar))
                    queryFiltro = queryFiltro.Where(x =>
                    x.PrimerNombre.Contains(entrada.Buscar) ||
                    x.SegundoNombre.Contains(entrada.Buscar) ||
                    x.PrimerApellido.Contains(entrada.Buscar) ||
                    x.SegundoApellido.Contains(entrada.Buscar) ||
                    x.NoCaso.ToString().Contains(entrada.Buscar) ||
                    x.NumeroIdentificacion.Contains(entrada.Buscar) ||
                    x.Usuario.Contains(entrada.Buscar));

                if (entrada.Orden == 1)
                    queryFiltro = queryFiltro.OrderByDescending(x => x.FechaUltimaActuacion);
                else if (entrada.Orden == 2)
                    queryFiltro = queryFiltro.OrderBy(x => x.FechaUltimaActuacion);

                var results = queryFiltro.ToList();

                if (results.Any())
                {
                    return new()
                    {
                        Estado = true,
                        Descripcion = "Consulta realizada con éxito.",
                        Datos = results.Select(x => new FiltroNNADto()
                        {
                            NoCaso = x.NoCaso,
                            IdNNA = x.NoCaso,
                            NombreNNA = x.NombreCompleto,
                            NoDocumento = x.NumeroIdentificacion,
                            UltimaActualizacion = x.FechaUltimaActuacion,
                            AgenteAsignado = x.Usuario,
                            EstadoId = x.Estado.Id,
                            Estado = x.Estado.Nombre,
                            EstadoDescripcion = x.Estado.Descripcion,
                            EstadoColorBG = x.Estado.ColorBG,
                            EstadoColorText = x.Estado.ColorText
                        }).ToList()
                    };
                }
                else
                {
                    return new()
                    {
                        Estado = true,
                        Descripcion = "No se encontraron resultados."
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new()
                {
                    Estado = false,
                    Descripcion = "Error al consultar los datos."
                };
            }
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

        public NNAResponse ConsultarNNAsById(long NNAId)
        {
            var response = new NNAResponse();

            try
            {
                var nnna = (from nna in _context.NNAs
                            where nna.Id == NNAId
                            select new NNAResponse()
                            {
                                NombreCompleto = string.Concat(nna.PrimerNombre, " ", nna.SegundoNombre, " ", nna.PrimerApellido, " ", nna.SegundoApellido),
                                Diagnostico = "",
                                FechaNacimiento = nna.FechaNacimiento,
                                Id = nna.Id
                            }).FirstOrDefault();

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
            try
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
                                                               FechaInicioSegumiento = seguimiento != null ? seguimiento.FechaSeguimiento : null,
                                                               FechaNacimiento = nna.FechaNacimiento,
                                                               NombreCompleto = $"{nna.PrimerNombre ?? ""} {nna.SegundoNombre ?? ""} {nna.PrimerApellido ?? ""} {nna.SegundoApellido ?? ""}",
                                                               DiagnosticoId = nna.DiagnosticoId
                                                           }).FirstOrDefaultAsync();

                if (response.DiagnosticoId != null)
                {
                    TPCIE10 cie10 = await _repositoryCie10.GetByIdAsync(response.DiagnosticoId.Value);

                    if (cie10 != null)
                    {
                        response.Diagnostico = cie10.Nombre;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
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

        public async Task<DepuracionProtocoloResponse> DepuracionProtocolo(List<DepuracionProtocoloRequest> request)
        {
            int recaidas = 0;
            int segundaNeoplasia = 0;
            int duplicados = 0;
            int ingresados = 0;

            List<ContactoNNA> insertContactoNNA = [];
            List<NNAs> insertNNA = [];
            List<NNAs> updateNNA = [];

            try
            {
                Dictionary<string, List<DepuracionProtocolo>> dNNAProtocolo = [];

                List<DepuracionProtocolo> listaDepuracion = [];

                for (int i = 0; i < request.Count; i++)
                {
                    DepuracionProtocolo depuracion = new()
                    {
                        Id = i,
                        DepuracionProtocoloRequest = request[i]
                    };

                    listaDepuracion.Add(depuracion);
                }

                // eliminar registros con ajuste 6 o d
                for (int i = listaDepuracion.Count - 1; i >= 0; i--)
                {
                    if (listaDepuracion[i].DepuracionProtocoloRequest.ajuste == "6" || listaDepuracion[i].DepuracionProtocoloRequest.ajuste == "D")
                    {
                        listaDepuracion.RemoveAt(i);
                    }
                }

                foreach (DepuracionProtocolo d in listaDepuracion)
                {
                    //por documento
                    if (dNNAProtocolo.ContainsKey(d.DepuracionProtocoloRequest.tip_ide + "&" + d.DepuracionProtocoloRequest.num_ide))
                    {
                        dNNAProtocolo[d.DepuracionProtocoloRequest.tip_ide + "&" + d.DepuracionProtocoloRequest.num_ide].Add(d);
                    }
                    else
                    {
                        dNNAProtocolo.Add(d.DepuracionProtocoloRequest.tip_ide + "&" + d.DepuracionProtocoloRequest.num_ide, new List<DepuracionProtocolo>());
                        dNNAProtocolo[d.DepuracionProtocoloRequest.tip_ide + "&" + d.DepuracionProtocoloRequest.num_ide].Add(d);
                    }

                    //por nombre 
                    if (dNNAProtocolo.ContainsKey(d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape))
                    {
                        dNNAProtocolo[d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape].Add(d);
                    }
                    else
                    {
                        dNNAProtocolo.Add(d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape, new List<DepuracionProtocolo>());
                        dNNAProtocolo[d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape].Add(d);
                    }

                    //por nombre 
                    if (dNNAProtocolo.ContainsKey(d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape))
                    {
                        dNNAProtocolo[d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape].Add(d);
                    }
                    else
                    {
                        dNNAProtocolo.Add(d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape, new List<DepuracionProtocolo>());
                        dNNAProtocolo[d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape].Add(d);
                    }
                }

                bool fallecido;
                int recorridoCaso2 = 0;
                int recorridoCaso3 = 0;
                DateTime fechaNotificacion;
                Dictionary<int, List<DateTime?>> dTrazabilidad;
                int cantidadMaxima = 0;
                bool anioActual;
                List<DepuracionProtocolo> depuracionManual;
                List<DepuracionProtocolo> insertarNNa;


                foreach (KeyValuePair<string, List<DepuracionProtocolo>> kvp in dNNAProtocolo)
                {
                    if (kvp.Value.Count > 1)
                    {
                        duplicados += 1;
                    }
                    else
                    {
                        ingresados += 1;
                    }
                }

                foreach (KeyValuePair<string, List<DepuracionProtocolo>> kvp in dNNAProtocolo)
                {
                    if (kvp.Value.Count > 1)
                    {
                        //priorizar fallecidos
                        fallecido = false;
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.con_fin == "2")
                            {
                                fallecido = true;
                            }
                        }

                        if (fallecido)
                        {
                            for (int i = kvp.Value.Count - 1; i >= 0; i--)
                            {
                                if (kvp.Value[i].DepuracionProtocoloRequest.con_fin != "2")
                                {
                                    kvp.Value.RemoveAt(i);
                                }
                            }
                        }
                    }

                    if (kvp.Value.Count > 1)
                    {
                        //recorrido del caso
                        recorridoCaso2 = 0;
                        recorridoCaso3 = 0;
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.tip_cas == "2")
                            {
                                recorridoCaso2 = 2;
                            }
                            else if (kvp.Value[i].DepuracionProtocoloRequest.ajuste == "3")
                            {
                                recorridoCaso3 = 3;
                            }
                        }

                        if (recorridoCaso2 == 2)
                        {
                            for (int i = kvp.Value.Count - 1; i >= 0; i--)
                            {
                                if (kvp.Value[i].DepuracionProtocoloRequest.tip_cas != "2")
                                {
                                    kvp.Value.RemoveAt(i);
                                }
                            }
                        }
                        else if (recorridoCaso3 == 3)
                        {
                            for (int i = kvp.Value.Count - 1; i >= 0; i--)
                            {
                                if (kvp.Value[i].DepuracionProtocoloRequest.tip_cas != "3")
                                {
                                    kvp.Value.RemoveAt(i);
                                }
                            }
                        }

                    }

                    if (kvp.Value.Count > 1)
                    {
                        //caso oportuno
                        fechaNotificacion = DateTime.Now;
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_not < fechaNotificacion)
                            {
                                fechaNotificacion = kvp.Value[i].DepuracionProtocoloRequest.fec_not;
                            }
                        }

                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_not != fechaNotificacion)
                            {
                                kvp.Value.RemoveAt(i);
                            }
                        }
                    }

                    if (kvp.Value.Count > 1)
                    {
                        dTrazabilidad = new Dictionary<int, List<DateTime?>>();
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            List<DateTime?> key = new();
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_initra != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_initra);
                            }
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_tomadp != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_tomadp);
                            }
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_res_dp != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_res_dp);
                            }
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_tomadd != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_tomadd);
                            }
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_res_dd != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_res_dd);
                            }
                            List<DateTime?> value;
                            if (!dTrazabilidad.TryGetValue(kvp.Value[i].Id, out value))
                            {
                                dTrazabilidad.Add(kvp.Value[i].Id, key);
                            }

                        }

                        cantidadMaxima = 0;
                        foreach (KeyValuePair<int, List<DateTime?>> t in dTrazabilidad)
                        {
                            if (cantidadMaxima < t.Value.Count)
                            {
                                cantidadMaxima = t.Value.Count;
                            }
                        }

                        List<DateTime?> date = null;
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            dTrazabilidad.TryGetValue(kvp.Value[i].Id, out date);

                            if (date.Count < cantidadMaxima)
                            {
                                kvp.Value.RemoveAt(i);
                            }
                        }
                    }


                    if (kvp.Value.Count > 1)
                    {
                        // Inicializar los indicadores de tipo de cáncer
                        bool tieneCancer1 = false, tieneCancer2 = false, tieneCancer3 = false;
                        bool tieneCancer4 = false, tieneCancer13 = false, tieneCancer14 = false;

                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            var tipoCa = kvp.Value[i].DepuracionProtocoloRequest.tipo_ca;

                            switch (tipoCa)
                            {
                                case "1": tieneCancer1 = true; break;
                                case "2": tieneCancer2 = true; break;
                                case "3": tieneCancer3 = true; break;
                                case "4": tieneCancer4 = true; break;
                                case "13": tieneCancer13 = true; break;
                                case "14": tieneCancer14 = true; break;
                            }
                        }

                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            var tipoCa = kvp.Value[i].DepuracionProtocoloRequest.tipo_ca;

                            if ((tieneCancer1 && tipoCa != "1") ||
                                (tieneCancer2 && tipoCa != "2") ||
                                (tieneCancer3 && tipoCa != "3") ||
                                (tieneCancer4 && tipoCa != "4") ||
                                (tieneCancer13 && tipoCa != "13") ||
                                (tieneCancer14 && tipoCa != "14"))
                            {
                                kvp.Value.RemoveAt(i);
                            }
                        }
                    }

                    if (kvp.Value.Count > 1)
                    {
                        anioActual = false;

                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_not.Year == DateTime.Now.Year ||
                                kvp.Value[i].DepuracionProtocoloRequest.recaida == "1" ||
                                kvp.Value[i].DepuracionProtocoloRequest.consx2_neo == "1")
                            {
                                anioActual = true;
                            }
                        }

                        if (anioActual)
                        {
                            for (int i = kvp.Value.Count - 1; i >= 0; i--)
                            {
                                if (kvp.Value[i].DepuracionProtocoloRequest.fec_not.Year != DateTime.Now.Year)
                                {
                                    kvp.Value.RemoveAt(i);
                                }
                            }
                        }
                    }
                }

                depuracionManual = new List<DepuracionProtocolo>();
                insertarNNa = new List<DepuracionProtocolo>();
                foreach (KeyValuePair<string, List<DepuracionProtocolo>> kvp in dNNAProtocolo)
                {
                    if (kvp.Value.Count > 1)
                    {
                        depuracionManual.AddRange(kvp.Value);
                    }
                    else
                    {
                        insertarNNa.AddRange(kvp.Value);
                    }
                }

                var clavesExistentes = _context.NNAs.Select(x => x.NumeroIdentificacion).ToHashSet();

                foreach (DepuracionProtocolo d in insertarNNa)
                {
                    if (d.DepuracionProtocoloRequest.consx2_neo == "1")
                    {
                        segundaNeoplasia += 1;
                    }
                    else if (d.DepuracionProtocoloRequest.recaida == "1")
                    {
                        recaidas += 1;
                    }

                    if (clavesExistentes.Contains(d.DepuracionProtocoloRequest.num_ide))
                    {
                        if (d.DepuracionProtocoloRequest.consx2_neo == "1" || d.DepuracionProtocoloRequest.recaida == "1")
                        {
                            var documento = d.DepuracionProtocoloRequest.num_ide;
                            var nna = await _context.NNAs.FirstOrDefaultAsync(x => x.NumeroIdentificacion == documento);

                            if (nna != null)
                            {
                                nna.TipoCancerId = d.DepuracionProtocoloRequest.tipo_ca;
                                nna.FechaDefuncion = DateTime.TryParse(d.DepuracionProtocoloRequest.fec_def, out DateTime fechaDefuncion) ? fechaDefuncion : DateTime.MinValue;
                                nna.MotivoDefuncion = d.DepuracionProtocoloRequest.cbmte;

                                if (d.DepuracionProtocoloRequest.recaida == "1")
                                    nna.Recaida = true;

                                updateNNA.Add(nna);
                            }
                        }
                        else
                        {
                            depuracionManual.Add(d);
                        }
                    }
                    else
                    {
                        var newNNA = new NNAs()
                        {
                            DateCreated = DateTime.Now,
                            CreatedByUserId = _user.ID.ToString(),
                            FechaNotificacionSIVIGILA = d.DepuracionProtocoloRequest.fec_not,
                            EPSId = 0,//verificar de donde sale el id de la eps
                            PrimerNombre = d.DepuracionProtocoloRequest.pri_nom,
                            SegundoNombre = d.DepuracionProtocoloRequest.seg_nom,
                            PrimerApellido = d.DepuracionProtocoloRequest.pri_ape,
                            SegundoApellido = d.DepuracionProtocoloRequest.seg_ape,
                            TipoIdentificacionId = d.DepuracionProtocoloRequest.tip_ide,
                            NumeroIdentificacion = d.DepuracionProtocoloRequest.num_ide,
                            SexoId = d.DepuracionProtocoloRequest.sexo,
                            PaisId = d.DepuracionProtocoloRequest.cod_pais_r,
                            ResidenciaOrigenMunicipioId = d.DepuracionProtocoloRequest.cod_mun_o,
                            ResidenciaOrigenAreaId = d.DepuracionProtocoloRequest.area,
                            ResidenciaOrigenBarrio = d.DepuracionProtocoloRequest.bar_ver,
                            ResidenciaOrigenDireccion = d.DepuracionProtocoloRequest.dir_res,
                            TipoRegimenSSId = d.DepuracionProtocoloRequest.tip_ss switch
                            {
                                "C" => "2",
                                "S" => "1",
                                "P" => "4",
                                "E" => "3",
                                "N" => "5",
                                "I" => "6",
                            },
                            EtniaId = d.DepuracionProtocoloRequest.per_etn,
                            ResidenciaOrigenEstratoId = d.DepuracionProtocoloRequest.estrato,
                            GrupoPoblacionId = "", //verificar como se asocia a un grupo poblacional
                            FechaConsultaDiagnostico = d.DepuracionProtocoloRequest.fec_con,
                            FechaInicioSintomas = d.DepuracionProtocoloRequest.ini_sin,
                            FechaHospitalizacion = d.DepuracionProtocoloRequest.fec_hos,
                            FechaDefuncion = DateTime.TryParse(d.DepuracionProtocoloRequest.fec_def, out DateTime fechaDefuncion) ? fechaDefuncion : DateTime.MinValue,
                            ResidenciaOrigenTelefono = d.DepuracionProtocoloRequest.telefono,
                            FechaNacimiento = d.DepuracionProtocoloRequest.fecha_nto,
                            MotivoDefuncion = d.DepuracionProtocoloRequest.cbmte,
                            TipoCancerId = d.DepuracionProtocoloRequest.tipo_ca,
                            FechaInicioTratamiento = DateTime.MinValue == d.DepuracionProtocoloRequest.fec_initra ? null : d.DepuracionProtocoloRequest.fec_initra,
                            Recaida = d.DepuracionProtocoloRequest.recaida == "1",
                            FechaDiagnostico = d.DepuracionProtocoloRequest.fec_diag1a,
                            CuidadorTelefono = d.DepuracionProtocoloRequest.tel_cont_2,
                            estadoId = 15,
                            FechaIngresoEstrategia = DateTime.Now,
                            OrigenReporteId = 1,
                            FechaConsultaOrigenReporte = DateTime.Now,
                            EstadoIngresoEstrategiaId = string.IsNullOrEmpty(d.DepuracionProtocoloRequest.fec_def) ? 1 : 2,
                            ResidenciaActualMunicipioId = d.DepuracionProtocoloRequest.nmun_resi,
                            DateUpdated = DateTime.Now,
                            UpdatedByUserId = _user.ID.ToString(),
                            DiagnosticoId = int.TryParse(d.DepuracionProtocoloRequest.tipo_ca, out int diagnosticoId) ? diagnosticoId : (int?)null,
                        };
                        insertNNA.Add(newNNA);

                        var contacto = new ContactoNNA()
                        {
                            NNAId = newNNA.Id,
                            Nombres = "Cuidador",
                            Telefonos = d.DepuracionProtocoloRequest.telefono,
                            Cuidador = true,
                        };
                        insertContactoNNA.Add(contacto);
                    }
                }

                _context.NNAs.UpdateRange(updateNNA);
                _context.NNAs.AddRange(insertNNA);
                _context.ContactoNNAs.AddRange(insertContactoNNA);
                await _context.SaveChangesAsync();

                List<DepuracionManualProtocolo> depuracionProtocolos = [];

                foreach (var d in depuracionManual)
                {
                    DepuracionManualProtocolo dep = new()
                    {
                        ajuste = d.DepuracionProtocoloRequest.ajuste,
                        anio = d.DepuracionProtocoloRequest.anio,
                        area = d.DepuracionProtocoloRequest.area,
                        bar_ver = d.DepuracionProtocoloRequest.bar_ver,
                        cbmte = d.DepuracionProtocoloRequest.cbmte,
                        cen_pobla = d.DepuracionProtocoloRequest.cen_pobla,
                        cer_def = d.DepuracionProtocoloRequest.cer_def,
                        cod_ase = d.DepuracionProtocoloRequest.cod_ase,
                        cod_dpto_o = d.DepuracionProtocoloRequest.cod_dpto_o,
                        cod_dpto_r = d.DepuracionProtocoloRequest.cod_dpto_r,
                        cod_eve = d.DepuracionProtocoloRequest.cod_eve,
                        cod_mun_o = d.DepuracionProtocoloRequest.cod_mun_o,
                        cod_mun_r = d.DepuracionProtocoloRequest.cod_mun_r,
                        cod_pais_o = d.DepuracionProtocoloRequest.cod_pais_o,
                        cod_pais_r = d.DepuracionProtocoloRequest.cod_pais_r,
                        cod_pre = d.DepuracionProtocoloRequest.cod_pre,
                        cod_sub = d.DepuracionProtocoloRequest.cod_sub,
                        consx2_neo = d.DepuracionProtocoloRequest.consx2_neo,
                        con_fin = d.DepuracionProtocoloRequest.con_fin,
                        crit_dx_de = d.DepuracionProtocoloRequest.crit_dx_de,
                        crit_dx_pr = d.DepuracionProtocoloRequest.crit_dx_pr,
                        dir_res = d.DepuracionProtocoloRequest.dir_res,
                        edad = d.DepuracionProtocoloRequest.edad,
                        estrato = d.DepuracionProtocoloRequest.estrato,
                        estrato_datos_complementarios = d.DepuracionProtocoloRequest.estrato_datos_complementarios,
                        FechaHora = d.DepuracionProtocoloRequest.FechaHora,
                        fecha_nto = d.DepuracionProtocoloRequest.fecha_nto,
                        fec_aju = d.DepuracionProtocoloRequest.fec_aju,
                        fec_arc_xl = d.DepuracionProtocoloRequest.fec_arc_xl,
                        fec_con = d.DepuracionProtocoloRequest.fec_con,
                        fec_def = d.DepuracionProtocoloRequest.fec_def,
                        fec_diag1a = d.DepuracionProtocoloRequest.fec_diag1a,
                        fec_hos = d.DepuracionProtocoloRequest.fec_hos,
                        fec_res_dd = d.DepuracionProtocoloRequest.fec_res_dd,
                        fec_initra = d.DepuracionProtocoloRequest.fec_initra,
                        fec_not = d.DepuracionProtocoloRequest.fec_not,
                        fec_res_dp = d.DepuracionProtocoloRequest.fec_res_dp,
                        fec_tomadd = d.DepuracionProtocoloRequest.fec_tomadd,
                        fec_tomadp = d.DepuracionProtocoloRequest.fec_tomadp,
                        fm_fuerza = d.DepuracionProtocoloRequest.fm_fuerza,
                        fm_grado = d.DepuracionProtocoloRequest.fm_grado,
                        fm_unidad = d.DepuracionProtocoloRequest.fm_unidad,
                        fuente = d.DepuracionProtocoloRequest.fuente,
                        gp_carcela = d.DepuracionProtocoloRequest.gp_carcela,
                        gp_desmovi = d.DepuracionProtocoloRequest.gp_desmovi,
                        gp_desplaz = d.DepuracionProtocoloRequest.gp_desplaz,
                        gp_discapa = d.DepuracionProtocoloRequest.gp_discapa,
                        gp_gestan = d.DepuracionProtocoloRequest.gp_gestan,
                        gp_indigen = d.DepuracionProtocoloRequest.gp_indigen,
                        gp_mad_com = d.DepuracionProtocoloRequest.gp_mad_com,
                        gp_migrant = d.DepuracionProtocoloRequest.gp_migrant,
                        gp_otros = d.DepuracionProtocoloRequest.gp_otros,
                        gp_pobicbf = d.DepuracionProtocoloRequest.gp_pobicbf,
                        gp_psiquia = d.DepuracionProtocoloRequest.gp_psiquia,
                        gp_vic_vio = d.DepuracionProtocoloRequest.gp_vic_vio,
                        ini_sin = d.DepuracionProtocoloRequest.ini_sin,
                        localidad = d.DepuracionProtocoloRequest.localidad,
                        nacionali = d.DepuracionProtocoloRequest.nacionali,
                        ndep_notif = d.DepuracionProtocoloRequest.ndep_notif,
                        ndep_proce = d.DepuracionProtocoloRequest.ndep_proce,
                        ndep_resi = d.DepuracionProtocoloRequest.ndep_resi,
                        nit_upgd = d.DepuracionProtocoloRequest.nit_upgd,
                        nmun_notif = d.DepuracionProtocoloRequest.nmun_notif,
                        nmun_proce = d.DepuracionProtocoloRequest.nmun_proce,
                        nmun_resi = d.DepuracionProtocoloRequest.nmun_resi,
                        nombre_nacionalidad = d.DepuracionProtocoloRequest.nombre_nacionalidad,
                        nom_dil_f = d.DepuracionProtocoloRequest.nom_dil_f,
                        nom_eve = d.DepuracionProtocoloRequest.nom_eve,
                        nom_grupo = d.DepuracionProtocoloRequest.nom_grupo,
                        nom_oncolo = d.DepuracionProtocoloRequest.nom_oncolo,
                        nom_upgd = d.DepuracionProtocoloRequest.nom_upgd,
                        npais_proce = d.DepuracionProtocoloRequest.npais_proce,
                        npais_resi = d.DepuracionProtocoloRequest.npais_proce,
                        num_ide = d.DepuracionProtocoloRequest.num_ide,
                        nuni_modif = d.DepuracionProtocoloRequest.nuni_modif,
                        ocupacion = d.DepuracionProtocoloRequest.ocupacion,
                        pac_hos = d.DepuracionProtocoloRequest.pac_hos,
                        per_etn = d.DepuracionProtocoloRequest.per_etn,
                        pri_ape = d.DepuracionProtocoloRequest.pri_ape,
                        pri_nom = d.DepuracionProtocoloRequest.pri_nom,
                        recaida = d.DepuracionProtocoloRequest.recaida,
                        seg_ape = d.DepuracionProtocoloRequest.seg_ape,
                        seg_nom = d.DepuracionProtocoloRequest.seg_nom,
                        semana = d.DepuracionProtocoloRequest.semana,
                        sem_ges = d.DepuracionProtocoloRequest.sem_ges,
                        sexo = d.DepuracionProtocoloRequest.sexo,
                        telefono = d.DepuracionProtocoloRequest.telefono,
                        tel_cont_2 = d.DepuracionProtocoloRequest.tel_cont_2,
                        tel_dil_f = d.DepuracionProtocoloRequest.tel_dil_f,
                        tel_oncolo = d.DepuracionProtocoloRequest.tel_oncolo,
                        tipo_ca = d.DepuracionProtocoloRequest.tipo_ca,
                        tip_cas = d.DepuracionProtocoloRequest.tip_cas,
                        tip_ide = d.DepuracionProtocoloRequest.tip_ide,
                        tip_ss = d.DepuracionProtocoloRequest.tip_ss,
                        uni_med = d.DepuracionProtocoloRequest.uni_med,
                        uni_modif = d.DepuracionProtocoloRequest.uni_modif,
                        vereda = d.DepuracionProtocoloRequest.vereda,
                        version = d.DepuracionProtocoloRequest.version,
                    };
                    depuracionProtocolos.Add(dep);
                }

                _context.DepuracionManualProtocolo.AddRange(depuracionProtocolos);
                await _context.SaveChangesAsync();

                ReporteDepuracion reporte = new()
                {
                    Estado = "Procesada",
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Hora = TimeOnly.FromDateTime(DateTime.Now),
                    Recaidas = recaidas,
                    RegistrosDuplicados = duplicados,
                    RegistrosIngresados = ingresados,
                    RegistrosNuevos = insertNNA.Count,
                    SegundasNeoplasias = segundaNeoplasia
                };

                _context.ReporteDepuracion.AddRange(reporte);
                await _context.SaveChangesAsync();

                await GenerarSeguimientos();
                var asignados = await _seguimientoRepo.AsignacionAutomatica();
                var reagendados = await _seguimientoRepo.AsignacionAutomaticaReagendar();
                var reasignados = await _seguimientoRepo.AsignacionAutomaticaReasignacion();

                var coordinadores = await _seguimientoRepo.CargarCoordinadores();
                var revisores = await _seguimientoRepo.CargarRevisores();

                if (coordinadores.Count() > 0)
                    await _notificacionRepo.EnviarNotificacionAsignacionCoordinadores(coordinadores.Select(x => x.Email).ToArray(), asignados, reagendados, reasignados);

                if (revisores.Count() > 0)
                {
                    asignados.AddRange(reagendados);
                    await _notificacionRepo.EnviarNotificacionAsignacionAgentes(revisores, asignados);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                ReporteDepuracion reporte = new()
                {
                    Estado = "Procesada",
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Hora = TimeOnly.FromDateTime(DateTime.Now),
                    Recaidas = recaidas,
                    RegistrosDuplicados = duplicados,
                    RegistrosIngresados = ingresados,
                    RegistrosNuevos = insertNNA.Count,
                    SegundasNeoplasias = segundaNeoplasia
                };

                _context.ReporteDepuracion.AddRange(reporte);
                await _context.SaveChangesAsync();
            }

            if (insertNNA.Count == 0)
                return new() { Estado = "Procesada" };
            else
                return new() { Estado = "Procesada", Nuevos = insertNNA.Count, Recaidas = recaidas, SegundasNeoplasias = segundaNeoplasia };
        }




        private async Task GenerarSeguimientos()
        {
            try
            {
                var estados = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 15 };

                var nnas = await (from nna in _context.NNAs
                                  join seg in _context.Seguimientos on nna.Id equals seg.NNAId into nnaSeguimientos // left join
                                  from nnaSeguimiento in nnaSeguimientos.DefaultIfEmpty()
                                  where nna.estadoId != null && estados.Contains(nna.estadoId ?? 0) && nnaSeguimiento == null
                                  select nna).ToListAsync();

                foreach (var nna in nnas)
                {
                    var seguimiento = new Seguimiento()
                    {
                        NNAId = nna.Id,
                        TieneDiagnosticos = true,
                        Telefono = nna.CuidadorTelefono,
                        ObservacionesSolicitante = "Generado automáticamente",
                        EstadoId = 1
                    };
                    _context.Seguimientos.Add(seguimiento);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _ = new Exception(ex.Message);
            }
        }

        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            NNAs? nna = null;
            if (seguimiento != null)
            {
                nna = (from nn in _context.NNAs
                       where nn.Id == seguimiento.NNAId
                       select nn).FirstOrDefault();
            }

            if (nna != null)
            {
                nna.ResidenciaOrigenMunicipioId = request.residenciaOrigen.IdMunicipio;
                nna.ResidenciaOrigenBarrio = request.residenciaOrigen.Barrio;
                nna.ResidenciaOrigenAreaId = request.residenciaOrigen.IdArea;
                nna.ResidenciaOrigenDireccion = request.residenciaOrigen.Direccion;
                nna.ResidenciaOrigenEstratoId = request.residenciaOrigen.IdEstrato;
                nna.ResidenciaOrigenTelefono = request.residenciaOrigen.TelefonoFijo;

                if (request.residenciaDestino != null)
                {
                    nna.ResidenciaActualMunicipioId = request.residenciaDestino.IdMunicipio;
                    nna.ResidenciaActualBarrio = request.residenciaDestino.Barrio;
                    nna.ResidenciaActualAreaId = request.residenciaDestino.IdArea;
                    nna.ResidenciaActualDireccion = request.residenciaDestino.Direccion;
                    nna.ResidenciaActualEstratoId = request.residenciaDestino.IdEstrato;
                    nna.ResidenciaActualTelefono = request.residenciaDestino.TelefonoFijo;
                }

                nna.TrasladoTieneCapacidadEconomica = request.CapacidadEconomicaTraslado;
                nna.TrasladoEAPBSuministroApoyo = request.ServiciosSocialesEAPB;
                nna.TrasladosServiciosdeApoyoOportunos = request.ServiciosSocialesEntregados;
                nna.TrasladosServiciosdeApoyoCobertura = request.ServiciosSocialesCobertura;
                nna.TrasladosHaSolicitadoApoyoFundacion = request.ApoyoRecibidoFundacion;
                nna.TrasladosNombreFundacion = request.NombreFundacion;
                nna.TrasladosPropietarioResidenciaActualId = request.IdTipoResidenciaActual;
                nna.TrasladosQuienAsumioCostosTraslado = request.AsumeCostoTraslado;
                nna.TrasladosQuienAsumioCostosVivienda = request.AsumeCostoVivienda;
                _context.NNAs.Update(nna);
                _context.SaveChanges();
            }
        }

        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            if (seguimiento != null)
            {
                NNAs? nna = (from nn in _context.NNAs
                             where nn.Id == seguimiento.NNAId
                             select nn).FirstOrDefault();

                if (nna != null)
                {

                    nna.DiagnosticoId = request.IdDiagnostico;
                    nna.FechaConsultaDiagnostico = request.FechaDiagnostico;
                    nna.FechaConsultaOrigenReporte = request.FechaConsulta;
                    nna.FechaInicioTratamiento = request.FechaInicioTratamiento;
                    nna.IPSId = request.IdIPS;
                    nna.Recaida = request.Recaidas;
                    nna.CantidadRecaidas = request.NumeroRecaidas;
                    nna.FechaUltimaRecaida = request.FechaUltimaRecaida;
                    nna.MotivoNoDiagnosticoId = request.IdMotivoNoDiagnostico;
                    nna.MotivoNoDiagnosticoOtro = request.RazonNoDiagnostico;
                }
            }
        }

        public void SetDificultadesProceso(DificultadesProcesoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            if (seguimiento != null)
            {
                NNAs? nna = (from nn in _context.NNAs
                             where nn.Id == seguimiento.NNAId
                             select nn).FirstOrDefault();

                if (nna != null)
                {
                    nna.DifAutorizaciondeMedicamentos = request.AutorizacionMedicamento;
                    nna.DifEntregaMedicamentosLAP = request.EntregaMedicamentoLAP;
                    nna.DifEntregaMedicamentosNoLAP = request.EntregaMedicamentoNoLAP;
                    nna.DifAsignaciondeCitas = request.AsignacionCitas;
                    nna.DifHanCobradoCuotasoCopagos = request.CobradoCopagos;
                    nna.DifAutorizacionProcedimientos = request.AutorizacionProcedimientos;
                    nna.DifRemisionInstitucionesEspecializadas = request.RemisionEspecialistas;
                    nna.DifMalaAtencionIPS = request.MalaAtencionIPS;
                    nna.DifMalaAtencionNombreIPSId = request.IdMalaIPS;
                    nna.DifFallasenMIPRES = request.FallasMIPRES;
                    nna.DifFallaConvenioEAPBeIPSTratante = request.FallasConvenio;
                    nna.CategoriaAlertaId = request.IdCategoriaAlerta;
                    nna.SubcategoriaAlertaId = request.IdSubcategoriaAlerta;
                    nna.TrasladosHaSidoTrasladadodeInstitucion = request.HaSidoTrasladado;
                    nna.TrasladosNumerodeTraslados = request.NumeroTraslados;
                    nna.TrasladosHaRecurridoAccionLegal = request.AccionLegal;
                    nna.TrasladosTipoAccionLegalId = request.IdTipoRecurso;
                    nna.TrasladosMotivoAccionLegal = request.MotivoAccionLegal;
                }
            }
        }

        public void SetAdherenciaProceso(AdherenciaProcesoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            if (seguimiento != null)
            {
                NNAs? nna = (from nn in _context.NNAs
                             where nn.Id == seguimiento.NNAId
                             select nn).FirstOrDefault();

                if (nna != null)
                {
                    nna.TratamientoHaDejadodeAsistir = request.HaDejadoTratamiento;
                    nna.TratamientoCuantoTiemposinAsistir = request.TiempoDejadoTratamiento;
                    nna.TratamientoUnidadMedidaIdTiempoId = request.IdUnidadTiempoDejadoTratamiento;
                    nna.TratamientoCausasInasistenciaId = request.IdCausaInasistenciaTratamiento;
                    nna.TratamientoCausasInasistenciaOtra = request.OtraCausaDejadoTratamiento;
                    nna.TratamientoEstudiaActualmente = request.EstudiaActualmente;
                    nna.TratamientoHaDejadodeAsistirColegio = request.HaDejadoEstudiar;
                    nna.TratamientoTiempoInasistenciaColegio = request.CuantoTiempoDejadoEstudiar;

                }
            }
        }

        public async Task<List<ConsultaCasosAbiertosResponse>> ConsultaCasosAbiertos(CasosAbiertosRequest request)
        {
            List<ConsultaCasosAbiertosResponse> response = new();
            List<ConsultaCasosAbiertosResponse> lista = new();
            List<int> estados = new() { 2, 3, 4, 5, 6, 7, 8, 9, 15, 16 };

            lista = (from seg in _context.Seguimientos
                     join nna in _context.NNAs on seg.NNAId equals nna.Id
                     where nna.estadoId.HasValue && estados.Contains(nna.estadoId.Value)
                     select new ConsultaCasosAbiertosResponse()
                     {
                         AsuntoUltimaActuacion = seg.UltimaActuacionAsunto,
                         Estado = seg.EstadoId,
                         FechaNotificacion = seg.FechaSolicitud ?? new(),
                         FechaUltimaActuacion = seg.UltimaActuacionFecha,
                         SeguimientoId = seg.Id
                     }).ToList();

            foreach (ConsultaCasosAbiertosResponse r in lista)
            {
                var alertas = await (from al in _context.AlertaSeguimientos
                                     join alerta in _context.Alertas on al.AlertaId equals alerta.Id
                                     where al.SeguimientoId == r.SeguimientoId
                                     select new AlertaSeguimientoResponse()
                                     {
                                         AlertaId = al.AlertaId,
                                         EstadoId = al.EstadoId,
                                         NombreAlerta = alerta.Descripcion,
                                         Observaciones = al.Observaciones,
                                         SeguimientoId = al.SeguimientoId,
                                         UltimaFechaSeguimiento = (DateTime)al.UltimaFechaSeguimiento
                                     }).ToArrayAsync();

                r.Alertas = alertas;
            }

            if (request.Filtro == "HOY")
            {
                response = (from re in lista
                            where re.FechaUltimaActuacion == DateTime.Now.Date
                            select re).ToList();
            }
            else if (request.Filtro == "ALERTA")
            {
                foreach (ConsultaCasosAbiertosResponse r in lista)
                {
                    if (r.Alertas != null && r.Alertas.Any())
                    {
                        response.Add(r);
                    }
                }
            }

            return response;

        }

        public void AsignacionManual(AsignacionManualRequest request)
        {
            UsuarioAsignado usuarioAsignado;
            List<UsuarioAsignado> usuarios = new();
            foreach (int i in request.Segumientos)
            {
                usuarioAsignado = new UsuarioAsignado()
                {
                    Activo = true,
                    DateCreated = DateTime.Now,
                    FechaAsignacion = DateTime.Now,
                    Observaciones = request.Motivo,
                    SeguimientoId = i,
                    UsuarioId = request.IdUsuario,
                };
                usuarios.Add(usuarioAsignado);
            }

            _context.UsuarioAsignados.AddRange(usuarios);
            _context.SaveChanges();
        }

        public async Task<DepuracionProtocoloResponse> CargarArchivoNNA(IFormFile file)
        {
            List<DepuracionProtocoloRequest> DepuracionRequest = [];
            DepuracionProtocoloResponse response;
            if (file == null)
                throw new ArgumentException("No se ha proporcionado un archivo.");

            if (file.Length == 0)
                throw new ArgumentException("El archivo está vacío.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyToAsync(stream);
                    var formatosValidos = new[] { ".xls", ".xlsx", ".csv" };
                    var extension = Path.GetExtension(file.FileName);
                    if (extension == null)
                        throw new ArgumentException("El archivo no tiene una extensión válida.");

                    if (!formatosValidos.Contains(extension.ToLower()))
                        throw new ArgumentException("El archivo no tiene un formato válido.");

                    if (extension.ToLower() == ".xls" || extension.ToLower() == ".xlsx")
                    {
                        using var workbook = new XLWorkbook(stream);
                        var worksheet = workbook.Worksheet(1); // Selecciona la primera hoja
                        var firstRow = worksheet.FirstRowUsed();
                        var lastRow = worksheet.LastRowUsed();

                        var columnCount = worksheet.Row(1).CellsUsed().Count();
                        if (columnCount != 98)
                            throw new ArgumentException("El archivo no tiene la cantidad de columnas correctas.");

                        if (lastRow.RowNumber() == 1)
                            throw new ArgumentException("El archivo no tiene registros.");

                        if (lastRow.RowNumber() > 501)
                            throw new ArgumentException("El archivo excede la cantidad de registros permitidos.");

                        // Recorrer las filas restantes
                        for (int row = firstRow.RowNumber() + 1; row <= lastRow.RowNumber(); row++)
                        {
                            var currentRow = worksheet.Row(row);
                            string xlSexo = currentRow.Cell(17).GetValue<string>();
                            if (xlSexo.Trim().ToLower().Equals("masculino") || xlSexo.Trim().ToLower().Equals("hombre"))
                            {
                                xlSexo = "H";
                            }
                            if (xlSexo.Trim().ToLower().Equals("femenino") || xlSexo.Trim().ToLower().Equals("mujer"))
                            {
                                xlSexo = "M";
                            }

                            DepuracionProtocoloRequest depuracion = new()
                            {
                                cod_eve = currentRow.Cell(1).GetValue<string>(),
                                fec_not = DateTime.TryParse(currentRow.Cell(2).GetValue<string>(), out DateTime fec_not) ? fec_not : DateTime.MinValue,
                                semana = currentRow.Cell(3).GetValue<int>(),
                                anio = currentRow.Cell(4).GetValue<int>(),
                                cod_pre = currentRow.Cell(5).GetValue<string>(),
                                cod_sub = currentRow.Cell(6).GetValue<string>(),
                                pri_nom = currentRow.Cell(7).GetValue<string>(),
                                seg_nom = currentRow.Cell(8).GetValue<string>(),
                                pri_ape = currentRow.Cell(9).GetValue<string>(),
                                seg_ape = currentRow.Cell(10).GetValue<string>(),
                                tip_ide = currentRow.Cell(11).GetValue<string>(),
                                num_ide = currentRow.Cell(12).GetValue<string>(),
                                edad = currentRow.Cell(13).GetValue<int>(),
                                uni_med = currentRow.Cell(14).GetValue<string>(),
                                nacionali = currentRow.Cell(15).GetValue<string>(),
                                nombre_nacionalidad = currentRow.Cell(16).GetValue<string>(),
                                sexo = xlSexo,
                                cod_pais_o = currentRow.Cell(18).GetValue<string>(),
                                cod_dpto_o = currentRow.Cell(19).GetValue<string>(),
                                cod_mun_o = currentRow.Cell(20).GetValue<string>(),
                                area = currentRow.Cell(21).GetValue<string>(),
                                localidad = currentRow.Cell(22).GetValue<string>(),
                                cen_pobla = currentRow.Cell(23).GetValue<string>(),
                                vereda = currentRow.Cell(24).GetValue<string>(),
                                bar_ver = currentRow.Cell(25).GetValue<string>(),
                                dir_res = currentRow.Cell(26).GetValue<string>(),
                                ocupacion = currentRow.Cell(27).GetValue<string>(),
                                tip_ss = currentRow.Cell(28).GetValue<string>(),
                                cod_ase = currentRow.Cell(29).GetValue<string>(),
                                per_etn = currentRow.Cell(30).GetValue<string>(),
                                nom_grupo = currentRow.Cell(31).GetValue<string>(),
                                estrato = currentRow.Cell(32).GetValue<string>(),
                                gp_discapa = currentRow.Cell(33).GetValue<string>(),
                                gp_desplaz = currentRow.Cell(34).GetValue<string>(),
                                gp_migrant = currentRow.Cell(35).GetValue<string>(),
                                gp_carcela = currentRow.Cell(36).GetValue<string>(),
                                gp_gestan = currentRow.Cell(37).GetValue<string>(),
                                sem_ges = currentRow.Cell(38).GetValue<string>(),
                                gp_indigen = currentRow.Cell(39).GetValue<string>(),
                                gp_pobicbf = currentRow.Cell(40).GetValue<string>(),
                                gp_mad_com = currentRow.Cell(41).GetValue<string>(),
                                gp_desmovi = currentRow.Cell(42).GetValue<string>(),
                                gp_psiquia = currentRow.Cell(43).GetValue<string>(),
                                gp_vic_vio = currentRow.Cell(44).GetValue<string>(),
                                gp_otros = currentRow.Cell(45).GetValue<string>(),
                                fuente = currentRow.Cell(46).GetValue<string>(),
                                cod_pais_r = currentRow.Cell(47).GetValue<string>(),
                                cod_dpto_r = currentRow.Cell(48).GetValue<string>(),
                                cod_mun_r = currentRow.Cell(49).GetValue<string>(),
                                fec_con = DateTime.TryParse(currentRow.Cell(50).GetValue<string>(), out DateTime fec_con) ? fec_con : DateTime.MinValue,
                                ini_sin = DateTime.TryParse(currentRow.Cell(51).GetValue<string>(), out DateTime ini_sin) ? ini_sin : DateTime.MinValue,
                                tip_cas = currentRow.Cell(52).GetValue<string>(),
                                pac_hos = currentRow.Cell(53).GetValue<string>(),
                                fec_hos = DateTime.TryParse(currentRow.Cell(54).GetValue<string>(), out DateTime fec_hos) ? fec_hos : DateTime.MinValue,
                                con_fin = currentRow.Cell(55).GetValue<String>(),
                                fec_def = currentRow.Cell(56).GetValue<string>(),
                                ajuste = currentRow.Cell(57).GetValue<string>(),
                                telefono = currentRow.Cell(58).GetValue<string>(),
                                fecha_nto = DateTime.TryParse(currentRow.Cell(59).GetValue<string>(), out DateTime fecha_nto) ? fecha_nto : DateTime.MinValue,
                                cer_def = currentRow.Cell(60).GetValue<string>(),
                                cbmte = currentRow.Cell(61).GetValue<string>(),
                                uni_modif = currentRow.Cell(62).GetValue<string>(),
                                nuni_modif = currentRow.Cell(63).GetValue<string>(),
                                fec_arc_xl = DateTime.TryParse(currentRow.Cell(64).GetValue<string>(), out DateTime fec_arc_xl) ? fec_arc_xl : DateTime.MinValue,
                                nom_dil_f = currentRow.Cell(65).GetValue<string>(),
                                tel_dil_f = currentRow.Cell(66).GetValue<string>(),
                                fec_aju = DateTime.TryParse(currentRow.Cell(67).GetValue<string>(), out DateTime fec_aju) ? fec_aju : DateTime.MinValue,
                                nit_upgd = currentRow.Cell(68).GetValue<string>(),
                                fm_fuerza = currentRow.Cell(69).GetValue<string>(),
                                fm_unidad = currentRow.Cell(70).GetValue<string>(),
                                fm_grado = currentRow.Cell(71).GetValue<string>(),
                                version = currentRow.Cell(72).GetValue<string>(),
                                tipo_ca = currentRow.Cell(73).GetValue<string>(),
                                fec_initra = DateTime.TryParse(currentRow.Cell(74).GetValue<string>(), out DateTime fec_initra) ? fec_initra : DateTime.MinValue,
                                consx2_neo = currentRow.Cell(75).GetValue<string>(),
                                recaida = currentRow.Cell(76).GetValue<string>(),
                                fec_diag1a = DateTime.TryParse(currentRow.Cell(77).GetValue<string>(), out DateTime fec_diag1a) ? fec_diag1a : DateTime.MinValue,
                                crit_dx_pr = currentRow.Cell(78).GetValue<string>(),
                                fec_tomadp = DateTime.TryParse(currentRow.Cell(79).GetValue<string>(), out DateTime fec_tomadp) ? fec_tomadp : DateTime.MinValue,
                                fec_res_dp = DateTime.TryParse(currentRow.Cell(80).GetValue<string>(), out DateTime fec_res_dp) ? fec_res_dp : DateTime.MinValue,
                                crit_dx_de = currentRow.Cell(81).GetValue<string>(),
                                fec_tomadd = DateTime.TryParse(currentRow.Cell(82).GetValue<string>(), out DateTime fec_tomadd) ? fec_tomadd : DateTime.MinValue,
                                fec_res_dd = DateTime.TryParse(currentRow.Cell(83).GetValue<string>(), out DateTime fec_res_dd) ? fec_res_dd : DateTime.MinValue,
                                nom_oncolo = currentRow.Cell(84).GetValue<string>(),
                                tel_oncolo = currentRow.Cell(85).GetValue<string>(),
                                tel_cont_2 = currentRow.Cell(86).GetValue<string>(),
                                estrato_datos_complementarios = currentRow.Cell(87).GetValue<string>(),
                                nom_eve = currentRow.Cell(88).GetValue<string>(),
                                nom_upgd = currentRow.Cell(89).GetValue<string>(),
                                npais_proce = currentRow.Cell(90).GetValue<string>(),
                                ndep_proce = currentRow.Cell(91).GetValue<string>(),
                                nmun_proce = currentRow.Cell(92).GetValue<string>(),
                                //depuracion.npais_proce = currentRow.Cell(93).GetValue<string>();
                                ndep_resi = currentRow.Cell(94).GetValue<string>(),
                                nmun_resi = currentRow.Cell(95).GetValue<string>(),
                                ndep_notif = currentRow.Cell(96).GetValue<string>(),
                                nmun_notif = currentRow.Cell(97).GetValue<string>(),
                                FechaHora = DateTime.TryParse(currentRow.Cell(98).GetValue<string>(), out DateTime FechaHora) ? FechaHora : DateTime.MinValue
                            };

                            DepuracionRequest.Add(depuracion);
                        }
                    }
                    else if (extension.ToLower() == ".csv")
                    {
                        var registros = new List<DepuracionProtocoloRequest>();

                        // Configurar el stream para leer desde el principio
                        stream.Seek(0, SeekOrigin.Begin);

                        using var reader = new StreamReader(stream);
                        string? line;
                        int lineNumber = 0;


                        while ((line = reader.ReadLine()) != null)
                        {
                            // Saltar encabezado
                            if (lineNumber == 0)
                            {
                                lineNumber++;
                                continue;
                            }

                            // Dividir la línea en columnas usando coma como separador
                            var columns = line.Split(',');
                            DepuracionProtocoloRequest depuracion = new()
                            {
                                cod_eve = columns[0],
                                fec_not = DateTime.TryParse(columns[1], out DateTime fec_not) ? fec_not : DateTime.MinValue,
                                semana = int.Parse(columns[2]),
                                anio = int.Parse(columns[3]),
                                cod_pre = columns[4],
                                cod_sub = columns[5],
                                pri_nom = columns[6],
                                seg_nom = columns[7],
                                pri_ape = columns[8],
                                seg_ape = columns[9],
                                tip_ide = columns[10],
                                num_ide = columns[11],
                                edad = Int32.Parse(columns[12]),
                                uni_med = columns[13],
                                nacionali = columns[14],
                                nombre_nacionalidad = columns[15],
                                sexo = columns[16],
                                cod_pais_o = columns[17],
                                cod_dpto_o = columns[18],
                                cod_mun_o = columns[19],
                                area = columns[20],
                                localidad = columns[21],
                                cen_pobla = columns[22],
                                vereda = columns[23],
                                bar_ver = columns[24],
                                dir_res = columns[25],
                                ocupacion = columns[26],
                                tip_ss = columns[27],
                                cod_ase = columns[28],
                                per_etn = columns[29],
                                nom_grupo = columns[30],
                                estrato = columns[31],
                                gp_discapa = columns[32],
                                gp_desplaz = columns[33],
                                gp_migrant = columns[34],
                                gp_carcela = columns[35],
                                gp_gestan = columns[36],
                                sem_ges = columns[37],
                                gp_indigen = columns[38],
                                gp_pobicbf = columns[39],
                                gp_mad_com = columns[40],
                                gp_desmovi = columns[41],
                                gp_psiquia = columns[42],
                                gp_vic_vio = columns[43],
                                gp_otros = columns[44],
                                fuente = columns[45],
                                cod_pais_r = columns[46],
                                cod_dpto_r = columns[47],
                                cod_mun_r = columns[48],
                                fec_con = DateTime.TryParse(columns[49], out DateTime fec_con) ? fec_con : DateTime.MinValue,
                                ini_sin = DateTime.TryParse(columns[50], out DateTime ini_sin) ? ini_sin : DateTime.MinValue,
                                tip_cas = columns[51],
                                pac_hos = columns[52],
                                fec_hos = DateTime.TryParse(columns[53], out DateTime fec_hos) ? fec_hos : DateTime.MinValue,
                                con_fin = columns[54],
                                fec_def = columns[55],
                                ajuste = columns[56],
                                telefono = columns[57],
                                fecha_nto = DateTime.TryParse(columns[58], out DateTime fecha_nto) ? fecha_nto : DateTime.MinValue,
                                cer_def = columns[59],
                                cbmte = columns[60],
                                uni_modif = columns[61],
                                nuni_modif = columns[62],
                                fec_arc_xl = DateTime.TryParse(columns[63], out DateTime fec_arc_xl) ? fec_arc_xl : DateTime.MinValue,
                                nom_dil_f = columns[64],
                                tel_dil_f = columns[65],
                                fec_aju = DateTime.TryParse(columns[66], out DateTime fec_aju) ? fec_aju : DateTime.MinValue,
                                nit_upgd = columns[67],
                                fm_fuerza = columns[68],
                                fm_unidad = columns[69],
                                fm_grado = columns[70],
                                version = columns[71],
                                tipo_ca = columns[72],
                                fec_initra = DateTime.TryParse(columns[73], out DateTime fec_initra) ? fec_initra : DateTime.MinValue,
                                consx2_neo = columns[74],
                                recaida = columns[75],
                                fec_diag1a = DateTime.TryParse(columns[76], out DateTime fec_diag1a) ? fec_diag1a : DateTime.MinValue,
                                crit_dx_pr = columns[77],
                                fec_tomadp = DateTime.TryParse(columns[78], out DateTime fec_tomadp) ? fec_tomadp : DateTime.MinValue,
                                fec_res_dp = DateTime.TryParse(columns[79], out DateTime fec_res_dp) ? fec_res_dp : DateTime.MinValue,
                                crit_dx_de = columns[80],
                                fec_tomadd = DateTime.TryParse(columns[81], out DateTime fec_tomadd) ? fec_tomadd : DateTime.MinValue,
                                fec_res_dd = DateTime.TryParse(columns[82], out DateTime fec_res_dd) ? fec_res_dd : DateTime.MinValue,
                                nom_oncolo = columns[83],
                                tel_oncolo = columns[84],
                                tel_cont_2 = columns[85],
                                estrato_datos_complementarios = columns[86],
                                nom_eve = columns[87],
                                nom_upgd = columns[88],
                                npais_proce = columns[89],
                                ndep_proce = columns[90],
                                nmun_proce = columns[91],
                                //depuracion.npais_proce = columns[92];
                                ndep_resi = columns[93],
                                nmun_resi = columns[94],
                                ndep_notif = columns[95],
                                nmun_notif = columns[96],
                                FechaHora = DateTime.TryParse(columns[97], out DateTime FechaHora) ? FechaHora : DateTime.MinValue
                            };

                            DepuracionRequest.Add(depuracion);
                        }
                    }
                }
                response = await DepuracionProtocolo(DepuracionRequest);
            }
            catch (Exception ex)
            {
                response = new DepuracionProtocoloResponse()
                {
                    Estado = $"Ocurrió un error al procesar el archivo: {ex.Message}"
                };
            }

            return response;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            return transaction;
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            await transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync(IDbContextTransaction transaction)
        {
            await transaction.RollbackAsync();
        }
    }
}