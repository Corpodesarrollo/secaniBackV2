namespace Core.Request
{
    public class NNASeguimientoRequest
    {
        public int NNAId { get; set; }
        public List<ContactoRequest> Contactos { get; set; }
        public int IdOrigenEstrategia { get; set; }
        public string? OtroOrigenEstrategia { get; set; }
        public string PrimerNombreNNA { get; set; }
        public string? SegundoNombreNNA { get; set; }
        public string PrimerApellidoNNA { get; set; }
        public string? SegundoApellidoNNA { get; set; }
        public string? IdTipoIdentificacionNNA { get; set; }
        public string NumeroIdentificacionNNA { get; set; }
        public DateTime FechaNacimientoNNA { get; set; }
        public string? IdPaisNacimientoNNA { get; set; }
        public string IdEtniaNNA { get; set; }
        public string? IdGrupoPoblacionalNNA { get; set; }
        public string? IdSexoNNA { get; set; }
        public string? IdRegimenAfiliacionNNA { get; set; }
        public int EAPBNNA { get; set; }

    }
}
