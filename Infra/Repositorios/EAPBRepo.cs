using Core.DTOs;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios
{
    public class EAPBRepo(ApplicationDbContext db) : IEAPBRepo
    {
        IQueryable<TPEAPBDto> GetSelect()
        {
            var query = from eapb in db.TPEAPB
                        select new TPEAPBDto
                        {
                            Id = eapb.Id,
                            Codigo = eapb.Codigo,
                            Nombre = eapb.Nombre,
                            Descripcion = eapb.Descripcion,
                            NIT = eapb.NIT,
                            DV = eapb.DV,
                            Creation = eapb.Creation,
                            LastUpdate = eapb.LastUpdate,
                            Tipo = eapb.Tipo
                        };
            return query;
        }

        public async Task<List<TPEAPBDto>> GetEAPB()
        {
            // BUG-LZ 2026-06-19: Tipo==2 filtraba solo EPS (381 filas) y excluia las Cajas
            // de Compensacion Familiar y otras EAPBs con Tipo NULL (99 filas, p.ej. "CAJA DE
            // COMPENSACION FAMILIAR COLSUBSIDIO" id=14053 que es la EAPB real de varios NNAs
            // en QA). Se incluyen ambos (480 entidades). Las IPS (Tipo==1, 1215 filas) siguen
            // excluidas porque este endpoint alimenta el catalogo de EAPBs, no de IPS.
            var query = GetSelect();
            var result = await query.Where(x => x.Tipo == 2 || x.Tipo == null).ToListAsync();
            return result;
        }

        public async Task<List<TPEAPBDto>> Search(string cadena)
        {
            var query = GetSelect();
            var result = await query.Where(x => x.Codigo.Contains(cadena) || x.Nombre.Contains(cadena)).Take(50).ToListAsync();
            return result;
        }

        public async Task<TPEAPBDto?> GetEAPBByCode(string code)
        {
            var query = GetSelect();
            var result = await query.Where(x => x.Codigo.Equals(code)).FirstOrDefaultAsync();
            return result;
        }

        public async Task<TPEAPBDto?> GetEAPBByCodeOrId(string value)
        {
            var query = GetSelect();
            var result = await query.Where(x => x.Codigo.Equals(value)).FirstOrDefaultAsync();
            if (result == null && int.TryParse(value, out var id))
            {
                result = await query.Where(x => x.Id == id).FirstOrDefaultAsync();
            }
            return result;
        }

        public async Task<TPEAPBDto?> GetEAPBById(int id)
        {
            var query = GetSelect();
            var result = await query.Where(x => x.Id == id).FirstOrDefaultAsync();
            return result;
        }

        public async Task<List<TPEAPBDto>> GetET()
        {
            var query = GetSelect();
            var result = await query.Where(x => x.Tipo == 1).ToListAsync();
            return result;
        }

        public async Task<List<TPEAPBDto>> Entidates()
        {
            var query = GetSelect();
            var result = await query.ToListAsync();
            return result;
        }
    }
}
