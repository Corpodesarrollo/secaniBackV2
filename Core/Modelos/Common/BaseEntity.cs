using Core.Interfaces.Repositorios.Common;

namespace Core.Modelos.Common
{
    public class BaseEntity : IBaseEntity
    {
        public long Id { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }
        public string? DeletedByUserId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
