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

        public List<GetIntentoContactoAgrupadoResponse> RepoIntentoContactoAgrupado(int NNAId)
        {
            List<GetIntentoContactoAgrupadoResponse> response = (from c in _context.ContactoNNAs
                            join i in _context.Intentos on c.Id equals i.ContactoNNAId into intentoGroup
                            from ig in intentoGroup.DefaultIfEmpty()
                            where c.NNAId == NNAId
                            group ig by new { c.Id, c.NNAId, c.Nombres, c.ParentescoId, c.Email, c.Telefonos, c.TelefnosInactivos, c.Cuidador } into grouped
                            select new GetIntentoContactoAgrupadoResponse
                            {
                                Id = grouped.Key.Id,
                                NNAId = grouped.Key.NNAId,
                                Nombres = grouped.Key.Nombres,
                                ParentescoId = grouped.Key.ParentescoId,
                                Email = grouped.Key.Email,
                                Telefonos = grouped.Key.Telefonos,
                                TelefnosInactivos = grouped.Key.TelefnosInactivos,
                                Cuidador = grouped.Key.Cuidador,
                                TipoResultadoIntento1 = grouped.Count(i => i != null && i.TipoResultadoIntentoId == 1),
                                TipoResultadoIntento2 = grouped.Count(i => i != null && i.TipoResultadoIntentoId == 2)
                            }).ToList();

            return response;
        }

        public List<GetContactoNNAIntentoResponse> RepoIntentosContactoNNA(int NNAId)
        {
            List<GetContactoNNAIntentoResponse> response = (from c in _context.ContactoNNAs
                                                            join i in _context.Intentos on c.Id equals i.ContactoNNAId
                                                            join t in _context.TPTipoFallaLLamada on i.TipoFallaIntentoId equals t.Id
                                                            where c.NNAId == NNAId
                                                            select new GetContactoNNAIntentoResponse
                                                            {
                                                                Id = c.Id,
                                                                NNAId = c.NNAId,
                                                                Nombres = c.Nombres,
                                                                ParentescoId = c.ParentescoId,
                                                                Email = c.Email,
                                                                Telefonos = c.Telefonos,
                                                                TelefnosInactivos = c.TelefnosInactivos,
                                                                Cuidador = c.Cuidador,
                                                                FechaIntento = i.FechaIntento,
                                                                TipoResultadoIntentoId = i.TipoResultadoIntentoId,
                                                                TipoFallaIntentoId = i.TipoFallaIntentoId,
                                                                TipoFallaIntentoNombre = t.Nombre,
                                                            }).ToList();

            return response;
        }


        public List<TPTipoFallaLLamada> RepoTipoFallas()
        {
            var response = _context.TPTipoFallaLLamada
                                    .Select(e => new TPTipoFallaLLamada
                                    {
                                        Id = e.Id,
                                        Nombre = e.Nombre,
                                        Descripcion = e.Descripcion,
                                    })
                                    .ToList();

            return response;
        }
    }
}
