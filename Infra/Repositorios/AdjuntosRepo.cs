using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;

namespace Infra.Repositorios
{
    public class AdjuntosRepo(ApplicationDbContext db) : IAdjuntosRepo
    {
        public async Task<AdjuntosDto?> GetById(int id)
        {
            var adjunto = await db.Adjuntos.FindAsync(id);
            if (adjunto == null)
                return null;

            return new()
            {
                Id = adjunto.Id,
                NombreArchivo = adjunto.NombreArchivo,
                Descripcion = adjunto.Descripcion,
                Url = adjunto.Url
            };
        }

        public async Task<long> AddAdjunto(AdjuntosDto adjunto)
        {
            var adjuntoEntity = new Adjuntos
            {
                NombreArchivo = adjunto.NombreArchivo,
                Descripcion = adjunto.Descripcion,
                Url = adjunto.Url
            };

            db.Adjuntos.Add(adjuntoEntity);
            await db.SaveChangesAsync();

            return adjuntoEntity.Id;
        }
    }
}
