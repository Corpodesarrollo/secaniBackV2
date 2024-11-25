using Infra.Repositories;
using Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Infra.Repositorios;
using Core.response;
using Core.Response;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;

namespace TestsSecani
{
    public class SeguimientoRepoTest
    {
        private readonly SeguimientoRepo SeguimientoRepo;
        private readonly ApplicationDbContext Context;
        private IWebHostEnvironment env;

        public SeguimientoRepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Test")
            .Options;

            env = Substitute.For<IWebHostEnvironment>();    

            Context = new ApplicationDbContext(options);
            SeguimientoRepo = new SeguimientoRepo(Context,env);
        }

        [Fact]
        public void GetSeguimientosNNA()
        {

            Context.Seguimientos.Add(new Core.Modelos.Seguimiento()
            {
                NNAId = 1,
                ObservacionAgente= "",
                ObservacionesSolicitante = ""
            });

            Context.NNAs.Add(new Core.Modelos.NNAs()
            {
                Id = 1,
                CreatedByUserId = ""
            });
            Context.SaveChanges();
            List<SeguimientoNNAResponse> response = SeguimientoRepo.GetSeguimientosNNA(1);

            Assert.NotNull(response);
            Assert.Single(response);
        }
        
    }
}
