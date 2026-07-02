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
using Core.Services.StorageService;
using Infra.Repositories.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SISPRO.TRV.Entity;
using System.Globalization;
using static Core.Common.Estructuras;


namespace Infra.Repositorios
{
    public class NNARepo : INNARepo
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<NNAs> _repository;
        private readonly GenericRepository<TPCIE10> _repositoryCie10;
        private readonly IEAPBRepo _eapbRepo;
        private readonly IIpsRepo _ipsRepo;
        private readonly INotificacionRepo _notificacionRepo;
        private readonly ISeguimientoRepo _seguimientoRepo;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IStorageService _storageService;
        private readonly User _user;

        public NNARepo(
            ApplicationDbContext context,
            ISeguimientoRepo seguimientoRepo,
            GenericRepository<NNAs> repository,
            GenericRepository<TPCIE10> repositoryCie10,
            IEAPBRepo eapbRepo,
            IIpsRepo ipsRepo,
            INotificacionRepo notificacionRepo,
            ICurrentUserProvider currentUserProvider,
            IStorageService storageService
            )
        {
            _context = context;
            _seguimientoRepo = seguimientoRepo;
            _repository = repository;
            _repositoryCie10 = repositoryCie10;
            _notificacionRepo = notificacionRepo;
            _currentUserProvider = currentUserProvider;
            _eapbRepo = eapbRepo;
            _ipsRepo = ipsRepo;
            _storageService = storageService;
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
                           TrasladosIPS = nna.TrasladosIPSId,
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
                if (nna != null)
                    nna.TrasladosIPSId = nna.TrasladosIPS?
                    .Split(",", StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToArray();
                return nna;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<(bool, NNAs)> AddAsync(NNAs entity, User user)
        {
            // BUG-028: preservar CreatedByUserId si viene del DTO (front qa-login envía localStorage user.id);
            // fallback a lookup por Alias (SISPRO real)
            if (string.IsNullOrEmpty(entity.CreatedByUserId))
            {
                var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Alias == user.Alias);
                entity.CreatedByUserId = usuario != null ? usuario.Id : "";
            }
            entity.DateCreated = DateTime.UtcNow;
            entity.estadoId = 15; // Registrado
            if (entity.OrigenReporteId == 1)
                entity.FechaNotificacionSIVIGILA = entity.FechaIngresoEstrategia;

            var eps = await _eapbRepo.GetEAPBById(entity.EAPBId ?? 0);
            if (eps != null)
            {
                entity.EPSId = eps.Id;
                entity.EAPBId = eps.Id;
            }

            var (success, response) = await _repository.AddAsync(entity);
            if (!success)
            {
                throw new KeyNotFoundException("cannot add entity");
            }
            return (success, response);
        }

        public async Task<(bool, NNAs)> UpdateAsync(NNAs entity, User user)
        {
            try
            {
                var usuario = await _context.Users.FirstOrDefaultAsync(u => u.Alias == user.Alias);
                entity.UpdatedByUserId = usuario != null ? usuario.Id : "";
                entity.DateUpdated = DateTime.UtcNow;
                var (success, response) = await _repository.UpdateAsync(entity);
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
                // BUG-LZ 2026-06-20: per HU SECANI-RQ01-HU02 deben listarse TODOS los NNA
                // registrados (con o sin seguimientos). Antes la query partia de Seguimientos
                // con INNER JOIN -> NNAs sin seguimiento quedaban ocultos. Ahora partimos de
                // NNAs y resolvemos el ultimo seguimiento y agente como subqueries que pueden
                // dar null sin excluir la fila.
                var queryResult = from n in _context.NNAs
                                  join e in _context.TPEstadoNNA on n.estadoId equals e.Id

                                  let lastSegId = _context.Seguimientos
                                      .Where(s => s.NNAId == n.Id)
                                      .OrderByDescending(s => s.Id)
                                      .Select(s => (long?)s.Id)
                                      .FirstOrDefault()

                                  let lastSeg = _context.Seguimientos
                                      .Where(s => s.Id == (lastSegId ?? 0))
                                      .FirstOrDefault()

                                  let lastUA = _context.UsuarioAsignados
                                      .Where(u => u.SeguimientoId == (lastSegId ?? 0))
                                      .OrderByDescending(u => u.Id)
                                      .FirstOrDefault()

                                  let agente = lastUA != null
                                      ? _context.Users.FirstOrDefault(usr => usr.Id == lastUA.UsuarioId)
                                      : null

                                  select new SeguimientoDto()
                                  {
                                      Id = lastSegId ?? 0,
                                      NoCaso = n.Id,
                                      PrimerNombre = n.PrimerNombre,
                                      SegundoNombre = n.SegundoNombre,
                                      PrimerApellido = n.PrimerApellido,
                                      SegundoApellido = n.SegundoApellido,
                                      NumeroIdentificacion = n.NumeroIdentificacion,
                                      FechaNotificacion = n.FechaNotificacionSIVIGILA,
                                      FechaSeguimiento = lastSeg != null ? lastSeg.FechaSeguimiento : (DateTime?)null,
                                      Estado = new TPEstadoNNADto()
                                      {
                                          Id = e.Id,
                                          Nombre = e.Nombre,
                                          Descripcion = e.Descripcion,
                                          ColorBG = e.ColorBG,
                                          ColorText = e.ColorText
                                      },
                                      AsuntoUltimaActuacion = lastSeg != null ? lastSeg.UltimaActuacionAsunto : null,
                                      FechaUltimaActuacion = lastSeg != null ? lastSeg.UltimaActuacionFecha : (DateTime?)null,
                                      UsuarioId = agente != null ? agente.Id : null,
                                      Usuario = agente != null ? agente.FullName : "",
                                      Alertas = (from als in _context.AlertaSeguimientos
                                                 join al in _context.Alertas on als.AlertaId equals al.Id
                                                 join ea in _context.TPEstadoAlerta on als.EstadoId equals ea.Id
                                                 join sca in _context.TPSubCategoriaAlerta on al.SubcategoriaId equals sca.Id
                                                 where als.SeguimientoId == (lastSegId ?? 0)
                                                 select new Core.DTOs.AlertaSeguimientoDto { Nombre = sca.CategoriaAlertaId + "." + sca.Indicador, Id = ea.Id }).ToList()
                                  };

                IQueryable<SeguimientoDto> queryFiltro = queryResult;
                if ((entrada.Estado ?? 0) > 0)
                    queryFiltro = queryResult.Where(x => x.Estado.Id == entrada.Estado);

                if (!string.IsNullOrEmpty(entrada.Agente))
                    queryFiltro = queryFiltro.Where(x => x.UsuarioId == entrada.Agente);

                if (entrada.Orden == 1)
                    queryFiltro = queryFiltro.OrderByDescending(x => x.FechaUltimaActuacion);
                else if (entrada.Orden == 2)
                    queryFiltro = queryFiltro.OrderBy(x => x.FechaUltimaActuacion);

                // BUG-LZ 2026-06-20: la projection con let/ternary no traduce a SQL cuando
                // se combina con Where(Contains). Materializamos primero y aplicamos el
                // filtro de busqueda en memoria.
                var resultsAll = queryFiltro.ToList();

                IEnumerable<SeguimientoDto> resultsFiltrados = resultsAll;
                if (!string.IsNullOrEmpty(entrada.Buscar))
                {
                    var b = entrada.Buscar;
                    resultsFiltrados = resultsAll.Where(x =>
                        (x.PrimerNombre ?? "").Contains(b, StringComparison.OrdinalIgnoreCase) ||
                        (x.SegundoNombre ?? "").Contains(b, StringComparison.OrdinalIgnoreCase) ||
                        (x.PrimerApellido ?? "").Contains(b, StringComparison.OrdinalIgnoreCase) ||
                        (x.SegundoApellido ?? "").Contains(b, StringComparison.OrdinalIgnoreCase) ||
                        (x.NoCaso?.ToString() ?? "").Contains(b) ||
                        (x.NumeroIdentificacion ?? "").Contains(b, StringComparison.OrdinalIgnoreCase) ||
                        (x.Usuario ?? "").Contains(b, StringComparison.OrdinalIgnoreCase));
                }

                var results = resultsFiltrados.ToList();

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
                            EstadoColorText = x.Estado.ColorText,
                            IdSeguimiento = x.Id
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
            // BUG-025-ext: contadores de exclusiones por mayor de edad / fallecido
            int excluidosMayorEdad = 0;
            int excluidosFallecidos = 0;

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
                    // BUG-025-ext: descartar fallecidos (con_fin == "2") y mayores de edad
                    if (d.DepuracionProtocoloRequest.con_fin == "2")
                    {
                        excluidosFallecidos++;
                        ingresados = Math.Max(0, ingresados - 1);
                        continue;
                    }
                    if (EsMayorDeEdadDesdeFecha(d.DepuracionProtocoloRequest.fecha_nto))
                    {
                        excluidosMayorEdad++;
                        ingresados = Math.Max(0, ingresados - 1);
                        continue;
                    }

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
                                nna.FechaDefuncion = ParseFechaSivigila(d.DepuracionProtocoloRequest.fec_def);
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
                        var eps = await _eapbRepo.GetEAPBByCode(d.DepuracionProtocoloRequest.cod_ase);
                        var ips = await _ipsRepo.GetIPSByCode(d.DepuracionProtocoloRequest.cod_pre);

                        var newNNA = new NNAs()
                        {
                            DateCreated = DateTime.Now,
                            CreatedByUserId = _user.ID.ToString(),
                            FechaNotificacionSIVIGILA = d.DepuracionProtocoloRequest.fec_not,
                            EAPBId = eps?.Id,
                            IPSId = ips?.Id,
                            PrimerNombre = d.DepuracionProtocoloRequest.pri_nom,
                            SegundoNombre = d.DepuracionProtocoloRequest.seg_nom,
                            PrimerApellido = d.DepuracionProtocoloRequest.pri_ape,
                            SegundoApellido = d.DepuracionProtocoloRequest.seg_ape,
                            TipoIdentificacionId = d.DepuracionProtocoloRequest.tip_ide,
                            NumeroIdentificacion = d.DepuracionProtocoloRequest.num_ide,
                            SexoId = d.DepuracionProtocoloRequest.sexo,
                            PaisId = d.DepuracionProtocoloRequest.cod_pais_r,
                            ResidenciaOrigenMunicipioId = NormalizeCodMun(d.DepuracionProtocoloRequest.cod_dpto_o, d.DepuracionProtocoloRequest.cod_mun_o),
                            ResidenciaOrigenAreaId = d.DepuracionProtocoloRequest.area,
                            ResidenciaOrigenBarrio = d.DepuracionProtocoloRequest.bar_ver,
                            ResidenciaOrigenDireccion = d.DepuracionProtocoloRequest.dir_res,
                            MunicipioNacimientoId = NormalizeCodMun(d.DepuracionProtocoloRequest.cod_dpto_r, d.DepuracionProtocoloRequest.cod_mun_r),
                            TipoRegimenSSId = (d.DepuracionProtocoloRequest.tip_ss ?? "").Trim().ToUpper() switch
                            {
                                // Codigos por letra (estandar SIVIGILA tradicional)
                                "C" => "2",
                                "S" => "1",
                                "P" => "4",
                                "E" => "3",
                                "N" => "5",
                                "I" => "6",
                                // Codigos numericos (variantes SIVIGILA actuales).
                                // 1=Subsidiado, 2=Contributivo, 3=Especial, 4=Particular,
                                // 5=No asegurado, 6=Indeterminado.
                                "1" => "1",
                                "2" => "2",
                                "3" => "3",
                                "4" => "4",
                                "5" => "5",
                                "6" => "6",
                                _ => null,
                            },
                            EtniaId = d.DepuracionProtocoloRequest.per_etn,
                            ResidenciaOrigenEstratoId = d.DepuracionProtocoloRequest.estrato,
                            DepartamentoNacimientoId = NormalizeCodDpto(d.DepuracionProtocoloRequest.cod_dpto_r),
                            GrupoPoblacionId = MapGrupoPoblacion(d.DepuracionProtocoloRequest),
                            FechaConsultaDiagnostico = d.DepuracionProtocoloRequest.fec_con,
                            FechaInicioSintomas = d.DepuracionProtocoloRequest.ini_sin,
                            FechaHospitalizacion = d.DepuracionProtocoloRequest.fec_hos,
                            FechaDefuncion = ParseFechaSivigila(d.DepuracionProtocoloRequest.fec_def),
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
                        _context.NNAs.Add(newNNA);
                        await _context.SaveChangesAsync();

                        var contacto = new ContactoNNA()
                        {
                            NNAId = newNNA.Id,
                            Nombres = "Cuidador",
                            Telefonos = d.DepuracionProtocoloRequest.telefono,
                            Cuidador = true,
                            Estado = true,
                            CreatedByUserId = _user.ID.ToString(),
                            DateCreated = DateTime.Now
                        };
                        _context.ContactoNNAs.Add(contacto);
                        await _context.SaveChangesAsync();

                        // BUG-013: crear Seguimiento + UsuarioAsignados automatico
                        var seguimiento = new Seguimiento()
                        {
                            NNAId = newNNA.Id,
                            FechaSeguimiento = DateTime.Now,
                            EstadoId = 1,
                            ContactoNNAId = contacto.Id,
                            Telefono = d.DepuracionProtocoloRequest.telefono,
                            UsuarioId = _user.ID.ToString(),
                            TieneDiagnosticos = !string.IsNullOrEmpty(d.DepuracionProtocoloRequest.tipo_ca),
                            UltimaActuacionAsunto = "Registro inicial",
                            UltimaActuacionFecha = DateTime.Now,
                            CreatedByUserId = _user.ID.ToString(),
                            DateCreated = DateTime.Now
                        };
                        _context.Seguimientos.Add(seguimiento);
                        await _context.SaveChangesAsync();

                        _context.UsuarioAsignados.Add(new UsuarioAsignado()
                        {
                            UsuarioId = _user.ID.ToString(),
                            SeguimientoId = seguimiento.Id,
                            FechaAsignacion = DateTime.Now,
                            Observaciones = "Asignación automática cargue SIVIGILA",
                            Activo = true,
                            CreatedByUserId = _user.ID.ToString(),
                            DateCreated = DateTime.Now
                        });
                        await _context.SaveChangesAsync();
                    }
                }

                _context.NNAs.UpdateRange(updateNNA);
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

                // HU RQ12-HU02: persistir detalle por NNA (TipoRegistro) para que el endpoint
                // ReporteDetalleRegDepurados pueda listar los NNAs procesados por cada cargue.
                var detalles = new List<ReporteDepuracionDetalle>();
                foreach (var n in insertNNA)
                    detalles.Add(new ReporteDepuracionDetalle { IdReporteDepuracion = reporte.Id, IdNNA = n.Id, TipoRegistro = (int)TipoRegistro.Nuevo });
                foreach (var n in updateNNA)
                {
                    var tipo = (n.Recaida == true)
                        ? (int)TipoRegistro.Recaida
                        : (int)TipoRegistro.Duplicado;
                    detalles.Add(new ReporteDepuracionDetalle { IdReporteDepuracion = reporte.Id, IdNNA = n.Id, TipoRegistro = tipo });
                }
                // Duplicados sin recaida/segunda-neoplasia van a depuracionManual (no a updateNNA).
                // Lookup IdNNA existente por num_ide para registrarlos en el detalle como Duplicado.
                if (depuracionManual.Count > 0)
                {
                    var numIdes = depuracionManual
                        .Select(x => x.DepuracionProtocoloRequest?.num_ide)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToList();
                    var nnasExistentes = await _context.NNAs
                        .Where(n => numIdes.Contains(n.NumeroIdentificacion))
                        .Select(n => new { n.Id, n.NumeroIdentificacion })
                        .ToListAsync();
                    foreach (var d in depuracionManual)
                    {
                        var numIde = d.DepuracionProtocoloRequest?.num_ide;
                        if (string.IsNullOrEmpty(numIde)) continue;
                        var nna = nnasExistentes.FirstOrDefault(x => x.NumeroIdentificacion == numIde);
                        if (nna == null) continue;
                        detalles.Add(new ReporteDepuracionDetalle { IdReporteDepuracion = reporte.Id, IdNNA = nna.Id, TipoRegistro = (int)TipoRegistro.Duplicado });
                    }
                }
                if (detalles.Count > 0)
                {
                    _context.ReporteDepuracionDetalle.AddRange(detalles);
                    await _context.SaveChangesAsync();
                }

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
                return new() { Estado = "Procesada", ExcluidosMayorEdad = excluidosMayorEdad, ExcluidosFallecidos = excluidosFallecidos };
            else
                return new() { Estado = "Procesada", Nuevos = insertNNA.Count, Recaidas = recaidas, SegundasNeoplasias = segundaNeoplasia, ExcluidosMayorEdad = excluidosMayorEdad, ExcluidosFallecidos = excluidosFallecidos };
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
                        UltimaActuacionAsunto = "Registro inicial",
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

        public async Task AsignacionManual(AsignacionManualRequest request)
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

            foreach (int i in request.Segumientos)
            {
                var noti = await _notificacionRepo.SetNotificacion(new()
                {
                    TipoNotificacion = TipoNotificacion.RespuestasNotificacionesAlertas,
                    IdAgenteOrigen = request.IdUsuarioOrigen,
                    IdAgenteDestino = request.IdUsuario,
                    IdSeguimiento = i
                });
            }
        }

        public async Task<DepuracionProtocoloResponse> CargarArchivoNNA(IFormFile file)
        {
            List<DepuracionProtocoloRequest> DepuracionRequest = [];
            DepuracionProtocoloResponse response;
            if (file == null)
                throw new ArgumentException("No se ha cargado ningún archivo. Seleccione un archivo .xls, .xlsx o .csv.");

            if (file.Length == 0)
                throw new ArgumentException("El archivo está vacío. Asegúrese de que contenga al menos un registro.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyToAsync(stream);
                    var formatosValidos = new[] { ".xlsx", ".csv" };
                    var extension = Path.GetExtension(file.FileName);
                    if (string.IsNullOrEmpty(extension))
                        throw new ArgumentException("El archivo no tiene una extensión válida. Solo se permiten archivos .xlsx o .csv.");

                    // Formato .xls (Excel 97-2003 BIFF) no soportado por ClosedXML. Mensaje claro
                    // para que el usuario re-guarde como .xlsx desde Excel.
                    if (extension.ToLower() == ".xls")
                        throw new ArgumentException("El formato .xls (Excel 97-2003) no es compatible. Abra el archivo en Excel y guarde como .xlsx antes de cargarlo.");

                    if (!formatosValidos.Contains(extension.ToLower()))
                        throw new ArgumentException("El formato del archivo no es válido. Solo se permiten archivos .xlsx o .csv.");

                    if (extension.ToLower() == ".xlsx")
                    {
                        using var workbook = new XLWorkbook(stream);
                        var worksheet = workbook.Worksheet(1); // Selecciona la primera hoja
                        var firstRow = worksheet.FirstRowUsed();
                        var lastRow = worksheet.LastRowUsed();

                        var columnCount = worksheet.Row(1).CellsUsed().Count();
                        if (columnCount != 98)
                            throw new ArgumentException($"El archivo debe contener exactamente 98 columnas. Se encontraron {columnCount}.");

                        if (lastRow.RowNumber() == 1)
                            throw new ArgumentException("El archivo no contiene registros. Debe incluir al menos un registro además del encabezado.");

                        if (lastRow.RowNumber() > 501)
                            throw new ArgumentException("El archivo excede el límite permitido de 500 registros.");

                        // RQ-10-HU01: validar nombres de columnas (98 esperados)
                        var headerCellsXls = Enumerable.Range(1, 98)
                            .Select(c => worksheet.Cell(1, c).GetValue<string>())
                            .ToList();
                        var erroresHeaderXls = ValidarNombresEncabezados(headerCellsXls);
                        if (erroresHeaderXls.Count > 0)
                            throw new ArgumentException("El archivo no cumple con los nombres de columnas requeridos. " + string.Join(" | ", erroresHeaderXls));

                        // Recorrer las filas restantes
                        for (int row = firstRow.RowNumber() + 1; row <= lastRow.RowNumber(); row++)
                        {
                            var currentRow = worksheet.Row(row);
                            string xlSexo = currentRow.Cell(17).GetValue<string>();
                            if (xlSexo.Trim().ToLower().Equals("masculino") || xlSexo.Trim().ToLower().Equals("hombre"))
                                xlSexo = "H";
                            else if (xlSexo.Trim().ToLower().Equals("femenino") || xlSexo.Trim().ToLower().Equals("mujer"))
                                xlSexo = "M";

                            if (!(new string[] { "M", "H" }).Contains(xlSexo))
                                xlSexo = "";

                            DepuracionProtocoloRequest depuracion = new()
                            {
                                cod_eve = currentRow.Cell(1).GetValue<string>(),
                                fec_not = ParseFechaSivigila(currentRow.Cell(2).GetValue<string>()),
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
                                fec_con = ParseFechaSivigila(currentRow.Cell(50).GetValue<string>()),
                                ini_sin = ParseFechaSivigila(currentRow.Cell(51).GetValue<string>()),
                                tip_cas = currentRow.Cell(52).GetValue<string>(),
                                pac_hos = currentRow.Cell(53).GetValue<string>(),
                                fec_hos = ParseFechaSivigila(currentRow.Cell(54).GetValue<string>()),
                                con_fin = currentRow.Cell(55).GetValue<String>(),
                                fec_def = currentRow.Cell(56).GetValue<string>(),
                                ajuste = currentRow.Cell(57).GetValue<string>(),
                                telefono = currentRow.Cell(58).GetValue<string>(),
                                fecha_nto = ParseFechaSivigila(currentRow.Cell(59).GetValue<string>()),
                                cer_def = currentRow.Cell(60).GetValue<string>(),
                                cbmte = currentRow.Cell(61).GetValue<string>(),
                                uni_modif = currentRow.Cell(62).GetValue<string>(),
                                nuni_modif = currentRow.Cell(63).GetValue<string>(),
                                fec_arc_xl = ParseFechaSivigila(currentRow.Cell(64).GetValue<string>()),
                                nom_dil_f = currentRow.Cell(65).GetValue<string>(),
                                tel_dil_f = currentRow.Cell(66).GetValue<string>(),
                                fec_aju = ParseFechaSivigila(currentRow.Cell(67).GetValue<string>()),
                                nit_upgd = currentRow.Cell(68).GetValue<string>(),
                                fm_fuerza = currentRow.Cell(69).GetValue<string>(),
                                fm_unidad = currentRow.Cell(70).GetValue<string>(),
                                fm_grado = currentRow.Cell(71).GetValue<string>(),
                                version = currentRow.Cell(72).GetValue<string>(),
                                tipo_ca = currentRow.Cell(73).GetValue<string>(),
                                fec_initra = ParseFechaSivigila(currentRow.Cell(74).GetValue<string>()),
                                consx2_neo = currentRow.Cell(75).GetValue<string>(),
                                recaida = currentRow.Cell(76).GetValue<string>(),
                                fec_diag1a = ParseFechaSivigila(currentRow.Cell(77).GetValue<string>()),
                                crit_dx_pr = currentRow.Cell(78).GetValue<string>(),
                                fec_tomadp = ParseFechaSivigila(currentRow.Cell(79).GetValue<string>()),
                                fec_res_dp = ParseFechaSivigila(currentRow.Cell(80).GetValue<string>()),
                                crit_dx_de = currentRow.Cell(81).GetValue<string>(),
                                fec_tomadd = ParseFechaSivigila(currentRow.Cell(82).GetValue<string>()),
                                fec_res_dd = ParseFechaSivigila(currentRow.Cell(83).GetValue<string>()),
                                nom_oncolo = currentRow.Cell(84).GetValue<string>(),
                                tel_oncolo = currentRow.Cell(85).GetValue<string>(),
                                tel_cont_2 = currentRow.Cell(86).GetValue<string>(),
                                estrato_datos_complementarios = currentRow.Cell(87).GetValue<string>(),
                                nom_eve = currentRow.Cell(88).GetValue<string>(),
                                nom_upgd = currentRow.Cell(89).GetValue<string>(),
                                npais_proce = currentRow.Cell(90).GetValue<string>(),
                                ndep_proce = currentRow.Cell(91).GetValue<string>(),
                                nmun_proce = currentRow.Cell(92).GetValue<string>(),
                                npais_resi = currentRow.Cell(93).GetValue<string>(),
                                ndep_resi = currentRow.Cell(94).GetValue<string>(),
                                nmun_resi = currentRow.Cell(95).GetValue<string>(),
                                ndep_notif = currentRow.Cell(96).GetValue<string>(),
                                nmun_notif = currentRow.Cell(97).GetValue<string>(),
                                FechaHora = ParseFechaSivigila(currentRow.Cell(98).GetValue<string>())
                            };

                            DepuracionRequest.Add(depuracion);
                        }
                    }
                    else if (extension.ToLower() == ".csv")
                    {
                        // Configurar el stream para leer desde el principio
                        stream.Seek(0, SeekOrigin.Begin);

                        using var reader = new StreamReader(stream);
                        var allLines = new List<string>();
                        string? rl;
                        while ((rl = reader.ReadLine()) != null) allLines.Add(rl);

                        if (allLines.Count == 0)
                            throw new ArgumentException("El archivo está vacío. Asegúrese de que contenga al menos un registro.");

                        var headerCols = allLines[0].Split(',');
                        if (headerCols.Length != 98)
                            throw new ArgumentException($"El archivo debe contener exactamente 98 columnas. Se encontraron {headerCols.Length}.");

                        if (allLines.Count == 1)
                            throw new ArgumentException("El archivo no contiene registros. Debe incluir al menos un registro además del encabezado.");

                        if (allLines.Count > 501)
                            throw new ArgumentException("El archivo excede el límite permitido de 500 registros.");

                        // RQ-10-HU01: validar nombres de columnas
                        var erroresHeaderCsv = ValidarNombresEncabezados(headerCols);
                        if (erroresHeaderCsv.Count > 0)
                            throw new ArgumentException("El archivo no cumple con los nombres de columnas requeridos. " + string.Join(" | ", erroresHeaderCsv));

                        for (int li = 1; li < allLines.Count; li++)
                        {
                            // Dividir la línea en columnas usando coma como separador
                            var columns = allLines[li].Split(',');
                            DepuracionProtocoloRequest depuracion = new()
                            {
                                cod_eve = columns[0],
                                fec_not = ParseFechaSivigila(columns[1]),
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
                                fec_con = ParseFechaSivigila(columns[49]),
                                ini_sin = ParseFechaSivigila(columns[50]),
                                tip_cas = columns[51],
                                pac_hos = columns[52],
                                fec_hos = ParseFechaSivigila(columns[53]),
                                con_fin = columns[54],
                                fec_def = columns[55],
                                ajuste = columns[56],
                                telefono = columns[57],
                                fecha_nto = ParseFechaSivigila(columns[58]),
                                cer_def = columns[59],
                                cbmte = columns[60],
                                uni_modif = columns[61],
                                nuni_modif = columns[62],
                                fec_arc_xl = ParseFechaSivigila(columns[63]),
                                nom_dil_f = columns[64],
                                tel_dil_f = columns[65],
                                fec_aju = ParseFechaSivigila(columns[66]),
                                nit_upgd = columns[67],
                                fm_fuerza = columns[68],
                                fm_unidad = columns[69],
                                fm_grado = columns[70],
                                version = columns[71],
                                tipo_ca = columns[72],
                                fec_initra = ParseFechaSivigila(columns[73]),
                                consx2_neo = columns[74],
                                recaida = columns[75],
                                fec_diag1a = ParseFechaSivigila(columns[76]),
                                crit_dx_pr = columns[77],
                                fec_tomadp = ParseFechaSivigila(columns[78]),
                                fec_res_dp = ParseFechaSivigila(columns[79]),
                                crit_dx_de = columns[80],
                                fec_tomadd = ParseFechaSivigila(columns[81]),
                                fec_res_dd = ParseFechaSivigila(columns[82]),
                                nom_oncolo = columns[83],
                                tel_oncolo = columns[84],
                                tel_cont_2 = columns[85],
                                estrato_datos_complementarios = columns[86],
                                nom_eve = columns[87],
                                nom_upgd = columns[88],
                                npais_proce = columns[89],
                                ndep_proce = columns[90],
                                nmun_proce = columns[91],
                                npais_resi = columns[92],
                                ndep_resi = columns[93],
                                nmun_resi = columns[94],
                                ndep_notif = columns[95],
                                nmun_notif = columns[96],
                                FechaHora = ParseFechaSivigila(columns[97])
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

        // BUG-012: Parse fechas SIVIGILA con formatos es-CO + normalizar codigos dpto/municipio
        private static readonly string[] _fechaFormats = new[]
        {
            "d/M/yyyy", "dd/MM/yyyy", "d/M/yyyy HH:mm", "dd/MM/yyyy HH:mm",
            "M/d/yyyy", "MM/dd/yyyy",
            "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss"
        };

        private static DateTime ParseFechaSivigila(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return DateTime.MinValue;
            var trimmed = s.Trim();
            if (DateTime.TryParseExact(trimmed, _fechaFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            if (DateTime.TryParse(trimmed, CultureInfo.GetCultureInfo("es-CO"), DateTimeStyles.None, out d))
                return d;
            return DateTime.MinValue;
        }

        // RQ-10-HU01 / HU04: nombres exactos de las 98 columnas SIVIGILA
        private static readonly string[] _expectedHeaders = new[]
        {
            "cod_eve","fec_not","semana","año","cod_pre","cod_sub",
            "pri_nom_","seg_nom_","pri_ape_","seg_ape_","tip_ide_","num_ide_",
            "edad_","uni_med_","nacionali_","nombre_nacionalidad","sexo_",
            "cod_pais_o","cod_dpto_o","cod_mun_o","area_","localidad_","cen_pobla_",
            "vereda_","bar_ver_","dir_res_","ocupacion_","tip_ss_","cod_ase_",
            "per_etn_","nom_grupo_","estrato_",
            "gp_discapa","gp_desplaz","gp_migrant","gp_carcela","gp_gestan",
            "sem_ges_","gp_indigen","gp_pobicbf","gp_mad_com","gp_desmovi",
            "gp_psiquia","gp_vic_vio","gp_otros","fuente_",
            "cod_pais_r","cod_dpto_r","cod_mun_r",
            "fec_con_","ini_sin_","tip_cas_","pac_hos_","fec_hos_","con_fin_",
            "fec_def_","ajuste_","telefono_","fecha_nto_","cer_def_","cbmte_",
            "uni_modif","nuni_modif","fec_arc_xl","nom_dil_f_","tel_dil_f_","fec_aju_",
            "nit_upgd","fm_fuerza","fm_unidad","fm_grado","version","tipo_ca",
            "fec_initra","consx2_neo","recaida","fec_diag1a",
            "crit_dx_pr","fec_tomadp","fec_res_dp","crit_dx_de","fec_tomadd","fec_res_dd",
            "nom_oncolo","tel_oncolo","tel_cont_2","estrato_datos_complementarios",
            "nom_eve","nom_upgd",
            "npais_proce","ndep_proce","nmun_proce","npais_resi","ndep_resi","nmun_resi",
            "ndep_notif","nmun_notif","FechaHora"
        };

        // Normaliza encabezados: quita espacios, guiones bajos finales, ignora mayusculas
        private static string NormalizeHeader(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return s.Trim().TrimEnd('_').ToLowerInvariant();
        }

        // RQ-10-HU01: compara encabezados leidos contra _expectedHeaders y reporta diferencias por columna
        private static List<string> ValidarNombresEncabezados(IEnumerable<string?> actual)
        {
            var actualList = actual.ToList();
            var errores = new List<string>();
            for (int i = 0; i < _expectedHeaders.Length; i++)
            {
                var encontrado = i < actualList.Count ? actualList[i] : null;
                if (NormalizeHeader(_expectedHeaders[i]) != NormalizeHeader(encontrado))
                {
                    errores.Add($"Columna {i + 1}: se esperaba '{_expectedHeaders[i]}' y se encontró '{encontrado ?? string.Empty}'.");
                }
            }
            return errores;
        }

        // BUG-025-ext: edad >= 18 desde DateTime de cargue masivo
        private static bool EsMayorDeEdadDesdeFecha(DateTime fechaNacimiento)
        {
            if (fechaNacimiento == DateTime.MinValue) return false;
            var hoy = DateTime.Now.Date;
            var edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad >= 18;
        }

        private static string NormalizeCodDpto(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s ?? string.Empty;
            var t = s.Trim();
            if (t.Length >= 2) return t.Substring(0, 2);
            return t.PadLeft(2, '0');
        }

        // BUG-012: Mapeo gp_* SIVIGILA → BiStgLCETipoPoblacionEspecial.codigo
        private static string MapGrupoPoblacion(DepuracionProtocoloRequest r)
        {
            if (r == null) return string.Empty;
            if (r.gp_discapa == "1") return "29";
            if (r.gp_desplaz == "1") return "24";
            if (r.gp_migrant == "1") return "30";
            if (r.gp_carcela == "1") return "14";
            if (r.gp_indigen == "1") return "17";
            if (r.gp_pobicbf == "1") return "25";
            if (r.gp_mad_com == "1") return "23";
            if (r.gp_desmovi == "1") return "8";
            if (r.gp_vic_vio == "1") return "9";
            return string.Empty;
        }

        private static string NormalizeCodMun(string? dpto, string? mun)
        {
            if (string.IsNullOrWhiteSpace(mun)) return mun ?? string.Empty;
            var m = mun.Trim();
            var dp = NormalizeCodDpto(dpto);
            if (m.Length >= 5) return m.Substring(0, 5);
            if (m.StartsWith(dp) && m.Length == 5) return m;
            return dp + m.PadLeft(3, '0');
        }

        public Task ActualizarFallecido(NNADto data)
        {
            //buscar ultimo seguimiento y cambiar estado a culminado [3]
            var seguimiento = (from seg in _context.Seguimientos
                               where seg.NNAId == data.Id
                               orderby seg.FechaSolicitud descending
                               select seg).FirstOrDefault();

            if (seguimiento != null)
            {
                seguimiento.ObservacionAgente = data.TratamientoObservaciones;
                seguimiento.EstadoId = 3; // Estado culminado
                _context.Seguimientos.Update(seguimiento);
                return _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("No se encontró un seguimiento para el NNA especificado.");
            }
        }

        // BUG-026: crear seguimiento inicial + UsuarioAsignados al crear NNA manual
        // (cargue masivo ya crea seguimiento; manual no lo hacía y por eso no aparecía
        // en Historico NNA, query une Seguimientos JOIN NNAs)
        public async Task CrearSeguimientoInicialNNA(long nnaId, long? contactoNNAId, string? telefono, string? userId)
        {
            var existe = await _context.Seguimientos.AnyAsync(s => s.NNAId == nnaId);
            if (existe) return;

            var seguimiento = new Seguimiento()
            {
                NNAId = nnaId,
                FechaSeguimiento = DateTime.Now,
                EstadoId = 1,
                ContactoNNAId = contactoNNAId ?? 0,
                Telefono = telefono ?? string.Empty,
                UsuarioId = userId ?? string.Empty,
                TieneDiagnosticos = false,
                UltimaActuacionAsunto = "Registro inicial",
                UltimaActuacionFecha = DateTime.Now,
                CreatedByUserId = userId ?? string.Empty,
                DateCreated = DateTime.Now
            };
            _context.Seguimientos.Add(seguimiento);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(userId))
            {
                _context.UsuarioAsignados.Add(new UsuarioAsignado()
                {
                    UsuarioId = userId,
                    SeguimientoId = seguimiento.Id,
                    FechaAsignacion = DateTime.Now,
                    Observaciones = "Asignación inicial creación manual",
                    Activo = true,
                    CreatedByUserId = userId,
                    DateCreated = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
        }

        // HU RQ07-HU08 (EAPB) + RQ09-HU10 (ET): listar casos pendientes reportar SIVIGILA.
        // FUENTE: ReportesSIVIGILA (los reportes hechos por cuidadores/externos via flujo
        // "Solicitar seguimiento") con Estado=0 (pendiente). Los NNAs validados que ya tienen
        // FechaNotificacionSIVIGILA=NULL no se incluyen aqui - esos son casos creados por agente,
        // no por reportante externo.
        // Filtros opcionales: eapbId (EAPB del usuario), municipioId (municipio), departamentoId (prefix DANE).
        public async Task<List<NNAPendienteSivigilaDto>> GetPendientesSivigila(int? eapbId, string? municipioId, string? departamentoId)
        {
            var q = from r in _context.ReportesSIVIGILA
                    where r.Estado == 0 && r.IsDeleted == false
                    join e in _context.TPEAPB on r.Aseguradora equals e.Id into eJ
                    from e in eJ.DefaultIfEmpty()
                    join m in _context.BiStgMunicipio on r.MunicipioProcedenciaId equals m.COD_MUNICIPIO into mJ
                    from m in mJ.DefaultIfEmpty()
                    select new
                    {
                        r.Id,
                        r.TipoIdentificacionId,
                        r.NumeroIdentificacion,
                        r.PrimerNombre,
                        r.SegundoNombre,
                        r.PrimerApellido,
                        r.SegundoApellido,
                        r.FechaNacimiento,
                        r.SexoId,
                        r.TieneDiagnostico,
                        r.Aseguradora,
                        r.MunicipioProcedenciaId,
                        r.DateCreated,
                        r.CreatedByUserId,
                        EAPBNombre = e != null ? e.Nombre : null,
                        MunicipioNombre = m != null ? m.Municipio : null
                    };

            if (eapbId.HasValue) q = q.Where(x => x.Aseguradora == eapbId.Value);
            if (!string.IsNullOrEmpty(municipioId)) q = q.Where(x => x.MunicipioProcedenciaId == municipioId);
            // ET filtra por departamento (prefix codigo DANE 2 digitos) -> ve toda su jurisdiccion
            if (!string.IsNullOrEmpty(departamentoId)) q = q.Where(x => x.MunicipioProcedenciaId != null && x.MunicipioProcedenciaId.StartsWith(departamentoId));

            var reportes = await q.OrderByDescending(x => x.Id).ToListAsync();

            // Resolver datos Reportante (Cuidador) via AspNetUsers por CreatedByUserId (Alias)
            var aliases = reportes
                .Where(x => !string.IsNullOrEmpty(x.CreatedByUserId) && x.CreatedByUserId != "Sistema")
                .Select(x => x.CreatedByUserId!)
                .Distinct()
                .ToList();
            var reportantesData = aliases.Any()
                ? await _context.Users
                    .Where(u => aliases.Contains(u.Id) || aliases.Contains(u.Alias!))
                    .Select(u => new ReportanteInfo(u.Id, u.Alias, u.FullName, u.Email, u.PhoneNumber))
                    .ToListAsync()
                : new List<ReportanteInfo>();
            // CreatedByUserId puede ser Id o Alias - indexar por ambos para hit garantizado
            var reportantesMap = new Dictionary<string, ReportanteInfo>();
            foreach (var u in reportantesData)
            {
                if (!string.IsNullOrEmpty(u.Id) && !reportantesMap.ContainsKey(u.Id)) reportantesMap[u.Id] = u;
                if (!string.IsNullOrEmpty(u.Alias) && !reportantesMap.ContainsKey(u.Alias)) reportantesMap[u.Alias] = u;
            }

            // Celular real del cuidador vive en ContactosAdicionalesCuidador.Tipo="celular_principal" (mi-perfil-cuidador)
            var userIds = reportantesData.Select(u => u.Id!).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            var celularesMap = userIds.Any()
                ? (await _context.ContactosAdicionalesCuidador
                    .Where(c => userIds.Contains(c.UserId!) && c.Tipo == "celular_principal" && !c.IsDeleted)
                    .Select(c => new { c.UserId, c.Valor })
                    .ToListAsync())
                    .GroupBy(c => c.UserId!)
                    .ToDictionary(g => g.Key, g => g.First().Valor)
                : new Dictionary<string, string?>();

            // Blob lookups 1 vez por tipo prefix
            var diagnosticosBlobs = await _storageService.ListFilesAsync("RS-EvidenciaDiagnostico-");
            var parentescosBlobs = await _storageService.ListFilesAsync("RS-EvidenciaParentesco-");

            string? FindArchivo(List<string> blobs, long reporteId)
            {
                var prefix = $"RS-Evidencia{(blobs == diagnosticosBlobs ? "Diagnostico" : "Parentesco")}-{reporteId}-";
                return blobs.FirstOrDefault(b => b.StartsWith(prefix));
            }

            return reportes.Select((r, i) =>
            {
                var reportanteKey = r.CreatedByUserId;
                ReportanteInfo? rep = !string.IsNullOrEmpty(reportanteKey) && reportanteKey != "Sistema" && reportantesMap.TryGetValue(reportanteKey, out var found)
                    ? found
                    : null;

                return new NNAPendienteSivigilaDto
                {
                    Id = r.Id,
                    NoCaso = i + 1,
                    TipoIdentificacionId = r.TipoIdentificacionId,
                    NumeroIdentificacion = r.NumeroIdentificacion,
                    NombreNnaCompleto = string.Join(" ", new[] { r.PrimerNombre, r.SegundoNombre, r.PrimerApellido, r.SegundoApellido }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    DiagnosticoSiNo = (r.TieneDiagnostico ?? false) ? "Si" : "No",
                    IdContacto = null,
                    NombreReportanteCompleto = rep?.FullName ?? (r.CreatedByUserId != "Sistema" ? r.CreatedByUserId : "Reportado por usuario externo"),
                    Aseguradora = r.EAPBNombre,
                    EAPBId = r.Aseguradora,
                    Municipio = r.MunicipioNombre,
                    MunicipioId = r.MunicipioProcedenciaId,
                    FechaConsultaOrigenReporte = r.DateCreated,
                    IdReporteSivigila = (int)r.Id,
                    ArchivoDiagnostico = FindArchivo(diagnosticosBlobs, r.Id),
                    ArchivoParentesco = FindArchivo(parentescosBlobs, r.Id),
                    FechaNacimientoNNA = r.FechaNacimiento,
                    SexoNNA = r.SexoId,
                    NombreReportante = rep?.FullName,
                    EmailReportante = rep?.Email,
                    CelularReportante = (rep != null && celularesMap.TryGetValue(rep.Id!, out var cel) ? cel : null) ?? rep?.PhoneNumber,
                    AliasReportante = rep?.Alias ?? (r.CreatedByUserId == "Sistema" ? null : r.CreatedByUserId),
                    // Parse Alias "CC9000000003" -> TipoId="CC" + NumeroId="9000000003"
                    TipoIdReportante = ParseTipoFromAlias(rep?.Alias),
                    NumeroIdReportante = ParseNumeroFromAlias(rep?.Alias)
                };
            }).ToList();
        }

        // Helpers para descomponer Alias formato {TIPO}{NUMERO} (CC9000000003, TI1234, etc)
        private static readonly System.Text.RegularExpressions.Regex _aliasRegex = new(@"^(CC|TI|CE|RC|PA|MS|AS|PE)(\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        private static string? ParseTipoFromAlias(string? alias)
        {
            if (string.IsNullOrEmpty(alias)) return null;
            var m = _aliasRegex.Match(alias);
            return m.Success ? m.Groups[1].Value.ToUpperInvariant() : null;
        }
        private static string? ParseNumeroFromAlias(string? alias)
        {
            if (string.IsNullOrEmpty(alias)) return null;
            var m = _aliasRegex.Match(alias);
            return m.Success ? m.Groups[2].Value : null;
        }
    }

    // record auxiliar para lookup reportante - anonymous types fallaban con dynamic cross-assembly
    public record ReportanteInfo(string? Id, string? Alias, string? FullName, string? Email, string? PhoneNumber);
}