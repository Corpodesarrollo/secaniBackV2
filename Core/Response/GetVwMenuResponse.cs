using MSAuthentication.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.response
{
    public class GetVwMenuResponse
    {
        public long? PermisoId { get; set; }
        public string? RoleId { get; set; }
        public string? RoleNombre { get; set; }
        public string? FuncionalidadNombre { get; set; }
        public int? MenuId { get; set; }
        public string? MenuNombre { get; set; }
        public string? MenuPath { get; set; }
        public string? MenuIcon { get; set; }
        public int? MenuOrden { get; set; }
        public int? MenuIdPadre { get; set; }
        public int? TieneSubMenu { get; set; }

        public List<MenuDto>? SubMenus { get; set; }
    }
}
