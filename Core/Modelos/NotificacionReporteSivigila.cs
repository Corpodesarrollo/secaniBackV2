using System.ComponentModel.DataAnnotations;

namespace Core.Modelos
{
    public class NotificacionReporteSivigila
    {
        [Key]
        public long Id { get; set; }
        public int? IdReporteSivigila { get; set; }
        public string? EntidadId { get; set; }
        public byte? TotalEnvios { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime? DateCreated { get; set; }

    }
}