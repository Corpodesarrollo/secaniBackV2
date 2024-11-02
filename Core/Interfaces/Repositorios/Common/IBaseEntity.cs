namespace Core.Interfaces.Repositorios.Common
{
    public interface IBaseEntity
    {
        DateTime? DateCreated { get; set; }
        string? CreatedByUserId { get; set; }
        DateTime? DateUpdated { get; set; }
        string? UpdatedByUserId { get; set; }
        DateTime? DateDeleted { get; set; }
        string? DeletedByUserId { get; set; }
        bool IsDeleted { get; set; }
    }
}
