using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class ResidenciaDiagnosticoTratamientoRequest
    {
        public ResidenciaRequest residenciaOrigen {  get; set; }
        public ResidenciaRequest? residenciaDestino {  get; set; }
        public int IdSeguimiento { get; set; }
        public bool CapacidadEconomicaTraslado { get; set; }
        public bool ServiciosSocialesEAPB { get; set; }
        public bool ServiciosSocialesEntregados { get; set; }
        public bool ServiciosSocialesCobertura { get; set; }
        public string? NombreFundacion { get; set; }
        public string? ApoyoRecibidoFundacion { get; set; }
        public int? IdTipoResidenciaActual { get; set; }
        public string? AsumeCostoTraslado { get; set; }
        public string? AsumeCostoVivienda { get; set; }
    }
}
