using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Repositorios
{
    public class SeguimientoRepo : ISeguimientoRepo
    {
        private readonly ApplicationDbContext _context;

        public SeguimientoRepo(ApplicationDbContext context)
        {
            _context = context;
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

            NNAs? nna=null;
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
    }
}
