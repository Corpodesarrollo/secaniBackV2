using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;
using Core.Modelos;
using MSAuthentication.Core.DTOs;

namespace Infra.Repositorios
{
    public class SeguimientoRepo : ISeguimientoRepo
    {
        private readonly ApplicationDbContext _context;

        public SeguimientoRepo(ApplicationDbContext context) => _context = context;


        public Seguimiento GetById(long id)
        {

            return _context.Seguimientos.FirstOrDefault(s => s.Id == id);
        }

        public List<GetSeguimientoResponse> RepoSeguimientoUsuario(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        {
            List<GetSeguimientoResponse> response = (from un in _context.Seguimientos
                                                     join nna in _context.NNAs on un.NNAId equals nna.Id

                                                     join alerta in _context.AlertaSeguimientos on un.Id equals alerta.SeguimientoId into alertaGroup
                                                     from subAlerta in alertaGroup.DefaultIfEmpty()

                                                     where un.UsuarioId == UsuarioId
                                                           && un.FechaSeguimiento >= FechaInicial
                                                           && un.FechaSeguimiento <= FechaFinal
                                                           && un.EstadoId != 3
                                                     group subAlerta by new
                                                     {
                                                         un.Id,
                                                         un.NNAId,
                                                         un.FechaSeguimiento,
                                                         un.EstadoId,
                                                         un.ContactoNNAId,
                                                         un.Telefono,
                                                         un.UsuarioId,
                                                         un.SolicitanteId,
                                                         un.FechaSolicitud,
                                                         un.TieneDiagnosticos,
                                                         un.ObservacionesSolicitante,
                                                         nna.PrimerNombre,
                                                         nna.SegundoNombre,
                                                         nna.PrimerApellido,
                                                         nna.SegundoApellido,
                                                         nna.FechaNotificacionSIVIGILA
                                                     } into g
                                                     select new GetSeguimientoResponse()
                                                     {
                                                         Id = g.Key.Id,
                                                         NNAId = g.Key.NNAId,
                                                         FechaSeguimiento = g.Key.FechaSeguimiento,
                                                         EstadoId = g.Key.EstadoId,
                                                         ContactoNNAId = g.Key.ContactoNNAId,
                                                         Telefono = g.Key.Telefono,
                                                         UsuarioId = g.Key.UsuarioId,
                                                         SolicitanteId = g.Key.SolicitanteId,
                                                         FechaSolicitud = g.Key.FechaSolicitud,
                                                         TieneDiagnosticos = g.Key.TieneDiagnosticos,
                                                         ObservacionesSolicitante = g.Key.ObservacionesSolicitante,
                                                         PrimerNombre = g.Key.PrimerNombre,
                                                         SegundoNombre = g.Key.SegundoNombre,
                                                         PrimerApellido = g.Key.PrimerApellido,
                                                         SegundoApellido = g.Key.SegundoApellido,
                                                         FechaNotificacionSIVIGILA = g.Key.FechaNotificacionSIVIGILA,
                                                         CantidadAlertas = g.Count(subAlerta => subAlerta != null)
                                                     }).ToList();



            return response;
        }

        public int RepoSeguimientoActualizacionFecha(PutSeguimientoActualizacionFechaRequest request)
        {

            var seguimiento = _context.Seguimientos.FirstOrDefault(s => s.Id == request.Id);

            if (seguimiento == null)
            {
                return -1;
            }

            seguimiento.FechaSeguimiento = request.FechaSeguimiento;

            _context.SaveChanges();
            return 1;
        }

        public int RepoSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request)
        {
            var seguimientoOriginal = _context.Seguimientos.FirstOrDefault(s => s.Id == request.Id);

            if (seguimientoOriginal == null)
            {
                return -1;
            }


            DateTime hoy = DateTime.Now;
            if (seguimientoOriginal.FechaSeguimiento < hoy)
            {
                return -2;
            }

            // Actualizar el EstadoId a cero
            seguimientoOriginal.EstadoId = 3;

            // Guardar los cambios en el seguimiento original
            _context.SaveChanges();

            try
            {

                var nuevoSeguimiento = new Seguimiento
                {
                    NNAId = seguimientoOriginal.NNAId,
                    FechaSeguimiento = seguimientoOriginal.FechaSeguimiento,

                    EstadoId = 1, // Valor inicial para el nuevo seguimiento (TODO: Definir por inexistencia de parametricas )

                    ContactoNNAId = seguimientoOriginal.ContactoNNAId,
                    Telefono = seguimientoOriginal.Telefono,

                    UsuarioId = request.UsuarioId, // Cambiar el valor del UsuarioId

                    SolicitanteId = seguimientoOriginal.SolicitanteId,
                    FechaSolicitud = seguimientoOriginal.FechaSolicitud,
                    TieneDiagnosticos = seguimientoOriginal.TieneDiagnosticos,

                    ObservacionesSolicitante = seguimientoOriginal.ObservacionesSolicitante + " " + request.ObservacionesSolicitante, // Nuevas observaciones

                    DateCreated = DateTime.Now,
                    CreatedByUserId = seguimientoOriginal.CreatedByUserId,
                    IsDeleted = false
                };

                _context.Seguimientos.Add(nuevoSeguimiento);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

                return -3;
            }

            return 1;
        }


        public List<GetSeguimientoFestivoResponse> RepoSeguimientoFestivo(DateTime FechaInicial, DateTime FechaFinal)
        {
            List<GetSeguimientoFestivoResponse> response = (from un in _context.TPFestivos
                                                            where
                                                                  un.Festivo >= FechaInicial
                                                                  && un.Festivo <= FechaFinal

                                                            select new GetSeguimientoFestivoResponse()
                                                            {

                                                                Festivo = un.Festivo,

                                                            }).ToList();



            return response;
        }


        public List<GetSeguimientoHorarioAgenteResponse> RepoSeguimientoHorarioAgente(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        {
            List<GetSeguimientoHorarioAgenteResponse> response = (from un in _context.HorarioLaboralAgente
                                                                  where
                                                                  un.UserId == UsuarioId
                                                                       && un.Fecha >= FechaInicial
                                                                       && un.Fecha <= FechaFinal

                                                                  select new GetSeguimientoHorarioAgenteResponse()
                                                                  {

                                                                      Fecha = un.Fecha,
                                                                      HoraEntrada = un.HoraEntrada,
                                                                      HoraSalida = un.HoraSalida,

                                                                  }).ToList();



            return response;
        }


        public List<GetSeguimientoAgentesResponse> RepoSeguimientoAgentes(string UsuarioId)
        {
            var response = (from ur in _context.AspNetUserRoles
                            join r in _context.AspNetRoles on ur.RoleId equals r.Id
                            join u in _context.AspNetUsers on ur.UserId equals u.Id
                            where r.Name.Contains("Agentes de seguimiento")
                            && u.Id != UsuarioId
                            select new GetSeguimientoAgentesResponse
                            {
                                Id = u.Id,
                                FullName = u.FullName!
                            }).ToList();

            return response;
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

        public void SetEstadoDiagnosticoTratamiento(EstadoDiagnosticoTratamientoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            if (seguimiento != null)
            {
                seguimiento.EstadoId = request.IdEstado;
                _context.Seguimientos.Update(seguimiento);
                _context.SaveChanges();
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
                nna.TrasladosHaSolicitadoApoyoFundacion = request.ApoyoFundacion;
                nna.TrasladosNombreFundacion = request.NombreFundacion;
                nna.TrasladosPropietarioResidenciaActualId = request.IdTipoResidenciaActual;
                nna.TrasladosQuienAsumioCostosTraslado = request.AsumeCostoTraslado;
                nna.TrasladosQuienAsumioCostosVivienda = request.AsumeCostoVivienda;
                _context.NNAs.Update(nna);
                _context.SaveChanges();
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

            if (seguimiento != null) {
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
    }
}
