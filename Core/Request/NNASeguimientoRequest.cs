using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class NNASeguimientoRequest
    {
        public int NNAId {  get; set; }
        public List<ContactoRequest> Contactos { get; set; }
        public int IdOrigenEstrategia { get; set; }
        public string? OtroOrigenEstrategia { get; set; }
        public string PrimerNombreNNA {  get; set; }
        public string? SegundoNombreNNA { get;set; }
        public string PrimerApellidoNNA { get; set; }
        public string? SegundoApellidoNNA { get; set; }
        public int IdTipoIdentificacionNNA { get; set; }
        public string NumeroIdentificacionNNA { get; set; }
        public DateTime FechaNacimientoNNA { get; set; }
        public int IdPaisNacimientoNNA {  get; set; }
        public int IdEtniaNNA { get; set; }
        public int IdGrupoPoblacionalNNA { get; set; }
        public int IdSexoNNA {  get; set; }
        public int IdRegimenAfiliacionNNA { get; set; }
        public int EAPBNNA { get;set;}

    }
}
