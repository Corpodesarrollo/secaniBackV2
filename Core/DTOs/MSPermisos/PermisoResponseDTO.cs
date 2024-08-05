using Core.Modelos;
using Core.Modelos.Common;

namespace Core.DTOs.MSPermisos
{
    public class PermisoResponseDTO : BaseEntity
    {
        public int FuncionalidadId { get; set; }
        public int? ModuloComponenteObjetoId { get; set; }
        public string RoleId { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDele { get; set; }
        public bool CanAdd { get; set; }
        public virtual TPFuncionalidad Funcionalidad { get; set; }
        public virtual TPModuloComponenteObjeto Modulo { get; set; }
    }
}
