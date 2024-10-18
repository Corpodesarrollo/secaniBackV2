using Core.Modelos;
using Core.Modelos.Identity;
using Core.Request;
using Infra;
using Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsSecani
{
    public class AlertaRepoTest
    {
        private readonly AlertaRepo AlertaRepo;
        private readonly ApplicationDbContext Context;

        public AlertaRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Test")
            .Options;

            Context = new ApplicationDbContext(options);
            AlertaRepo = new AlertaRepo(Context);
        }

        [Fact]
        public void CrearAlerta_Exitoso()
        {
            Context.Users.Add(new ApplicationUser { Id = "prueba1", FullName = "Giovanny Romero", Telefonos = "", UserName = "user.externo@yopmail.com" });

            Context.SaveChanges();
            // Inicializar datos de prueba
            CrearAlertaSeguimientoRequest request = new()
            {
                AlertaId = 1,
                EstadoId = 2,
                Observaciones = "observacion",
                SeguimientoId = 1,
            };
            string response = AlertaRepo.CrearAlertaSeguimiento(request);

            Assert.NotNull(response);
            Assert.Equal("Alerta creada exitosamente", response);
        }

        [Fact]
        public void GestionarAlerta_Exitoso()
        {
            Context.Users.Add(new ApplicationUser { Id = "prueba2", FullName = "Giovanny Romero", Telefonos = "", UserName = "user.externo@yopmail.com" });

            Context.SaveChanges();
            // Inicializar datos de prueba
            GestionarAlertaRequest request = new()
            {
                IdAlerta = 1,
                IdEstado = 2,
                IdSeguimiento = 1,
                Observacion = "observacion",
                UserName = "user.externo@yopmail.com"
            };
            string response = AlertaRepo.GestionarAlerta(request);

            Assert.NotNull(response);
            Assert.Equal("Seguimiento actualizado exitosamente", response);
        }

        [Fact]
        public void ConsultarAlertaSeguimiento_Exitoso()
        {
            Context.AlertaSeguimientos.Add(new AlertaSeguimiento() { SeguimientoId = 11, AlertaId = 1, CreatedByUserId = "", Observaciones = "" });

            Context.SaveChanges();
            // Inicializar datos de prueba
            ConsultarAlertasRequest request = new()
            {
                IdSeguimiento = 11
            };
            List<AlertaSeguimiento> response = AlertaRepo.ConsultarAlertaSeguimiento(request);

            Assert.NotNull(response);
            Assert.Equal(2, response.Count);
        }

        [Fact]
        public void ConsultarAlertaEstados_Exitoso()
        {
            Context.AlertaSeguimientos.Add(new AlertaSeguimiento() { SeguimientoId = 11, AlertaId = 1, CreatedByUserId = "", Observaciones = "", EstadoId = 1 });

            Context.SaveChanges();
            // Inicializar datos de prueba
            ConsultarAlertasEstadosRequest request = new()
            {
                estados = [1]
            };
            List<AlertaSeguimiento> response = AlertaRepo.ConsultarAlertaEstados(request);

            Assert.NotNull(response);
            Assert.Single(response);
        }
    }
}
