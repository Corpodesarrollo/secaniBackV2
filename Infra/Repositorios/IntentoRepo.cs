using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;
using Core.Modelos;
using MSAuthentication.Core.DTOs;


namespace Infra.Repositorios
{
    public class IntentoRepo : IIntentoRepo
    {
        private readonly ApplicationDbContext _context;

        public IntentoRepo(ApplicationDbContext context) => _context = context;

        public List<GetIntentoResponse> RepoIntentoUsuario(int UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        {
            List<GetIntentoResponse> response = (from intento in _context.Intentos
                                                 where intento.CreatedByUserId == UsuarioId.ToString()
                                                       && intento.FechaIntento >= FechaInicial
                                                       && intento.FechaIntento <= FechaFinal
                                                 select new GetIntentoResponse()
                                                 {
                                                     Id = intento.Id,
                                                     ContactoNNAId = intento.ContactoNNAId,
                                                     Email = intento.Email,
                                                     FechaIntento = intento.FechaIntento,
                                                     Telefono = intento.Telefono,
                                                     TipoResultadoIntentoId = intento.TipoResultadoIntentoId,
                                                     TipoFallaIntentoId = intento.TipoFallaIntentoId,
                                                     DateDeleted = intento.DateDeleted,
                                                     DateUpdated = intento.DateUpdated,
                                                     CreatedByUserId = intento.CreatedByUserId,
                                                     DateCreated = intento.DateCreated,
                                                     DeletedByUserId = intento.DeletedByUserId,
                                                     IsDeleted = intento.IsDeleted,
                                                     UpdatedByUserId = intento.UpdatedByUserId
                                                 }).ToList();

            return response;
        }

        public int RepoInsertarIntento(PostIntentoRequest request)
        {
            var intento = new Intentos
            {
                ContactoNNAId = request.ContactoNNAId,
                Email = request.Email,
                FechaIntento = DateTime.Now,
                Telefono = request.Telefono,
                TipoResultadoIntentoId = request.TipoResultadoIntentoId,
                TipoFallaIntentoId = request.TipoFallaIntentoId,
                CreatedByUserId = request.CreatedByUserId!,
                DateCreated = DateTime.Now,
                IsDeleted = false
            };

            _context.Intentos.Add(intento);
            return _context.SaveChanges();
        }

        public int RepoIntentoActualizacionFecha(PutIntentoActualizacionFechaRequest request)
        {
            var intento = _context.Intentos.FirstOrDefault(i => i.Id == request.Id);

            if (intento == null)
            {
                return -1;
            }

            intento.FechaIntento = request.FechaIntento;
            intento.DateUpdated = DateTime.Now;

            _context.SaveChanges();
            return 1;
        }

        public int RepoIntentoActualizacionUsuario(PutIntentoActualizacionUsuarioRequest request)
        {
            var intento = _context.Intentos.FirstOrDefault(i => i.Id == request.Id);

            if (intento == null)
            {
                return -1;
            }

            intento.CreatedByUserId = request.UsuarioId.ToString();
            intento.DateUpdated = DateTime.Now;

            _context.SaveChanges();
            return 1;
        }
    }
}
