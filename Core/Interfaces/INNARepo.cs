using Core.DTOs;
using Core.Modelos;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositorios
{
    public interface INNARepo
    {
        public RespuestaResponse<ContactoNNA> CrearContactoNNA(ContactoNNA contactoNNA);

        public RespuestaResponse<ContactoNNA> ActualizarContactoNNA(ContactoNNA contactoNNA);

        public RespuestaResponse<ContactoNNA> ObtenerContactoPorId(long NNAId);

        public RespuestaResponse<FiltroNNADto> ConsultarNNAFiltro(FiltroNNARequest entrada);

        public List<TPEstadoNNA> TpEstadosNNA();

        public List<VwAgentesAsignados> VwAgentesAsignados();

        public void ActualizarNNASeguimiento(NNASeguimientoRequest request);

        public List<TPTipoIdentificacionDto> GetTpTipoId();
        public List<TPTipoIdentificacionDto> GetTPTipoIdentificacion();

        public List<TPRegimenAfiliacionDto> GetTPRegimenAfiliacion();
        public List<TPParentescoDto> GetTPParentesco();
        public List<TPPaisDto> GetTPPais();
        public List<TPDepartamentoDto> GetTPDepartamento(int PaisId);
        public List<TPCiudadDto> GetTPCiudad(int DepartamentoId);
        public List<TPOrigenReporteDto> GetTPOrigenReporte();
        public List<TPGrupoPoblacionalDto> GetGrupoPoblacional();
        public List<TPEtniaDto> GetTPEtnia();
        public List<TPEAPBDto> GetTPEAPB();
        public List<TPEstadoIngresoEstrategiaDto> GetTPEstadoIngresoEstrategia();
        public NNAs ConsultarNNAsByTipoIdNumeroId(int tipoIdentificacionId, string numeroIdentificacion);
        public NNAs ConsultarNNAsById(long NNAId );
        public Task<DatosBasicosNNAResponse> ConsultarDatosBasicosNNAById(long  NNAId, TablaParametricaService tablaParametricaService);
        public Task<SolicitudSeguimientoCuidadorResponse> SolicitudSeguimientoCuidador(long NNAId, TablaParametricaService tablaParametricaService);
    }
}
