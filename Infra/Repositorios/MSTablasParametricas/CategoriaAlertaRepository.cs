using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Infra.Repositories.Common;
using Mapster;

namespace Infra.Repositorios.MSTablasParametricas
{
    public class CategoriaAlertaRepository(ApplicationDbContext context) : GenericRepository<TPCategoriaAlerta>(context), ICategoriaAlertaRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<CategoriaAlertaDTO> GetCategoriaAlertaWithSubCategorias(int id, CancellationToken cancellationToken)
        {
            var categoriaAlerta = _context.TPCategoriaAlerta.FirstOrDefault(c => c.Id == id);

            if (categoriaAlerta == null)
            {
                return await Task.FromResult<CategoriaAlertaDTO>(null);
            }

            // BUG-LZ-067: filtraba por Sub.Id == Cat.Id (PK == PK) en lugar de FK
            // CategoriaAlertaId. Por eso solo retornaba 1 subcategoria por categoria (la
            // que coincidia accidentalmente por Id). Fix: usar la FK correcta.
            // BUG-LZ-070: ademas filtrar IsDeleted y Activo para no retornar test data
            // ("SUBCAT PRUEBA", "Item nuevo") que estaban borradas logicamente.
            var subCategorias = _context.TPSubCategoriaAlerta
                .Where(c => c.CategoriaAlertaId == categoriaAlerta.Id
                            && !c.IsDeleted
                            && c.Activo);

            var categoriaAlertaDTO = categoriaAlerta.Adapt<CategoriaAlertaDTO>();
            categoriaAlertaDTO.SubCategorias = subCategorias.Adapt<List<SubCategoriaAlertaDTO>>();
            return categoriaAlertaDTO;
        }
    }
}
