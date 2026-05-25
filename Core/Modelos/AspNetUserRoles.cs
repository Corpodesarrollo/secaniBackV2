using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Modelos.Common;

namespace Core.Modelos
{
    [Table("AspNetUserRoles")]
    public class AspNetUserRoles
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public AspNetUsers? User { get; set; }

        public string? RoleId { get; set; }

    }
}
