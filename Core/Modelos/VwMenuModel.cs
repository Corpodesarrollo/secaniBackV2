using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Modelos

{
    [Table("VwMenu")]
    public class VwMenuModel
    {        
        public long PermisoId { get; set; }
        public string RoleId { get; set; }
        public string RoleNombre { get; set; }
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
