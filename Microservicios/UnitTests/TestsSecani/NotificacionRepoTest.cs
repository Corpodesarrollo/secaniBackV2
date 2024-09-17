using Core.Modelos;
using Core.response;
using Infra.Repositories;
using Infra;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Request;

namespace TestsSecani
{
    public class NotificationRepoTest
    {
        private readonly NotificacionRepo NotificacionRepo;
        private readonly ApplicationDbContext Context;

        public NotificationRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Test")
            .Options;

            Context = new ApplicationDbContext(options);
            NotificacionRepo = new NotificacionRepo(Context);
        }

        [Fact]
        public void GetNotificacionUsuario_ReturnLista()
        {
            // Inicializar datos de prueba
            Context.AspNetUsers.Add(new AspNetUsers { Id = "prueba4", FullName = "Giovanny Romero", Telefonos = "" });

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
            Context.AspNetUsers.Add(new AspNetUsers { Id = "prueba3", FullName = "Giovanny Romero", Telefonos = "" });

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
