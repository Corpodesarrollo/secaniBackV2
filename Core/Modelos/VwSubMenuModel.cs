using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Modelos

{
    [Table("VwSubMenu")]
    public class VwSubMenuModel
    {        
        public long PermisoId { get; set; }
        public String RoleId { get; set; }
        public String RoleNombre { get; set; }
        public string FuncionalidadNombre { get; set; }
        public int MenuId { get; set; }
        public string MenuNombre { get; set; }
        public string MenuPath { get; set; }
        public string MenuIcon { get; set; }
        public int MenuOrden { get; set; }
        public int MenuIdPadre { get; set; }
        public int TieneSubMenu { get; set; }
    }
}
