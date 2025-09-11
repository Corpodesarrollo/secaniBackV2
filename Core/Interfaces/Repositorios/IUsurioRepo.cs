using Core.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositorios
{
    public interface IUsurioRepo
    {
        public UltimoRol UltimoRolPorIdUsuario(string IdUsuario);
    }
}
