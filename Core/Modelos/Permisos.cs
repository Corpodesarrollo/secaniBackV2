namespace Core.Modelos
{
    public class Permisos
    {
        public long Id { get; set; }
        public DateTime? DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateDeleted { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }
        public string? DeletedByUserId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int FuncionalidadId { get; set; }
        public int? ModuloComponenteObjetoId { get; set; }
        public string RoleId { get; set; }
        public bool? CanView { get; set; }
        public bool? CanEdit { get; set; }
        public bool? CanDele { get; set; }
        public bool? CanAdd { get; set; }
    }
}
