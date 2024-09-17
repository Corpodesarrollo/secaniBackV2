using Infra.Repositorios;
using Infra;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Services.MSTablasParametricas;
using NSubstitute;
using Core.Response;
using Core.Modelos.Common;
using System.Net;

namespace TestsSecani
{
    public class NNARepoTest
    {
        private readonly NNARepo NNARepo;
        private readonly ApplicationDbContext Context;
        private TablaParametricaService tablaParametricaService;

        public NNARepoTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "Test")
            .Options;

            Context = new ApplicationDbContext(options);
            NNARepo = new NNARepo(Context);
        }

        //[Fact]
        //public async void SolicitudSeguimientoCuidador()
        //{
        //    List<TPExternalEntityBase> listaParametrica = new List<TPExternalEntityBase>();
        //    var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
        //    {
        //        Content = new StringContent("{'data':'value'}")
        //    };
        //    var fakeHandler = new FakeHttpMessageHandler(fakeResponse);
        //    var httpClient = new HttpClient(fakeHandler);

        //    tablaParametricaService = new TablaParametricaService(httpClient);
        //    tablaParametricaService.GetBynomTREFCodigo("CIE10", "1", CancellationToken.None).Returns(listaParametrica);
        //    Context.NNAs.Add(new Core.Modelos.NNAs()
        //    {
        //        Id = 1,
        //        DiagnosticoId = "1",
        //        CreatedByUserId = ""
        //    });
        //    Context.SaveChanges();
        //    SolicitudSeguimientoCuidadorResponse response = await NNARepo.SolicitudSeguimientoCuidador(1,tablaParametricaService);

        //    Assert.NotNull(response);
        //}
    }
}
