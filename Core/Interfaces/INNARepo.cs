using Core.DTOs;
using Core.Modelos;
using Core.Request;
using Core.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositorios
{
    public interface INNARepo
    {
        public NNAs GetById(long id);
        public RespuestaResponse<ContactoNNA> CrearContactoNNA(ContactoNNA contactoNNA);

        public RespuestaResponse<ContactoNNA> ActualizarContactoNNA(ContactoNNA contactoNNA);

        public RespuestaResponse<ContactoNNA> ObtenerContactoPorId(long NNAId);

        public RespuestaResponse<FiltroNNADto> ConsultarNNAFiltro(FiltroNNARequest entrada);

        public List<TPEstadoNNA> TpEstadosNNA();

        public List<VwAgentesAsignados> VwAgentesAsignados();

        public void ActualizarNNASeguimiento(NNASeguimientoRequest request);
       
    }
}
