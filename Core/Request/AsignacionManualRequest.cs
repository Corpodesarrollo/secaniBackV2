namespace Core.Request
{
    public class AsignacionManualRequest
    {
        public string IdUsuarioOrigen { get; set; }
        public string IdUsuario { get; set; }
        public string Motivo { get; set; }
        public List<int> Segumientos { get; set; }
    }
}
