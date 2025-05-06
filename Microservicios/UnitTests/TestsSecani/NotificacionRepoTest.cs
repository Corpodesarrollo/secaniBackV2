using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Identity;
using Core.response;
using Core.Services.StorageService;
using Infra;
using Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace TestsSecani
{
    public class NotificationRepoTest
    {
        private readonly NotificacionRepo NotificacionRepo;
        private readonly ApplicationDbContext Context;
        private IAdjuntosRepo adjuntosRepo;
        private IStorageService storageService;

        public NotificationRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Test")
            .Options;
            adjuntosRepo = Substitute.For<IAdjuntosRepo>();
            storageService = Substitute.For<IStorageService>();

            Context = new ApplicationDbContext(options);
            //NotificacionRepo = new NotificacionRepo(Context,adjuntosRepo,storageService, new ReportesSIVIGILARepo(Context, storageService));
        }

        [Fact]
        public void GetNotificacionUsuario_ReturnLista()
        {
            // Inicializar datos de prueba
            Context.Users.Add(new ApplicationUser { Id = "prueba4", FullName = "Giovanny Romero", Telefonos = "" });

            Context.NotificacionesUsuarios.Add(new NotificacionesUsuario
            {
                AgenteDestinoId = "prueba4",
                AgenteOrigenId = "prueba4",
                SeguimientoId = 123,
                IsDeleted = false,
                Accion = "",
                CreatedByUserId = "giovanny.romero"
            });

            Context.SaveChanges();
            List<GetNotificacionResponse> response = NotificacionRepo.GetNotificacionUsuario("prueba4");

            Assert.NotNull(response);
            Assert.Single(response);
        }

        [Fact]
        public void GetNumeroNotificacionUsuario_ReturnUno()
        {
            Context.Users.Add(new ApplicationUser { Id = "prueba3", FullName = "Giovanny Romero", Telefonos = "" });

            Context.NotificacionesUsuarios.Add(new NotificacionesUsuario
            {
                AgenteDestinoId = "prueba3",
                AgenteOrigenId = "prueba3",
                SeguimientoId = 123,
                IsDeleted = false,
                Accion = "",
                CreatedByUserId = "giovanny.romero"
            });

            Context.SaveChanges();
            int response = NotificacionRepo.GetNumeroNotificacionUsuario("prueba3");
            Assert.Equal(1, response);
        }
    }
}
