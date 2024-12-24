using ClosedXML.Excel;
using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Modelos.TablasParametricas;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Infra.Repositories.Common;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace Infra.Repositorios
{
    public class NNARepo : INNARepo
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<NNAs> _repository;
        private readonly GenericRepository<TPCIE10> _repositoryCie10;


        public NNARepo(ApplicationDbContext context)
        {
            _context = context;
            GenericRepository<NNAs> repository = new(_context);
            _repository = repository;
            GenericRepository<TPCIE10> repositoryCie10 = new(_context);
            _repositoryCie10 = repositoryCie10;
        }

        public async Task<NNADto?> GetById(long id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Entity not found");

                NNADto nNADto = result.Adapt<NNADto>();

                Entidad? eps = (from ent in _context.Entidades
                                where ent.Id == nNADto.EPSId
                                select ent).FirstOrDefault();
                if (eps != null)
                {
                    nNADto.EPSNombre = eps.Nombre;
                }

                Entidad? ips = (from ent in _context.Entidades
                                where ent.Id == nNADto.IPSId
                                select ent).FirstOrDefault();

                if (ips != null)
                {
                    nNADto.IPSNombre = ips.Nombre;
                }

                Entidad? eapb = (from ent in _context.Entidades
                                 where ent.Id == nNADto.EAPBId
                                 select ent).FirstOrDefault();

                if (eapb != null)
                {
                    nNADto.EAPBNombre = eapb.Nombre;
                }

                return nNADto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<(bool, NNAs)> AddAsync(NNAs entity)
        {
            var (success, response) = await _repository.AddAsync(entity);
            if (!success)
            {
                throw new KeyNotFoundException("cannot add entity");
            }
            return (success, response);
        }

        public async Task<(bool, NNAs)> UpdateAsync(NNAs entity)
        {
            try
            {
                var (success, response) = await _repository.UpdateAsync(entity);
                if (!success)
                {
                    throw new KeyNotFoundException("cannot update entity");
                }
                return (success, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

        }

        public List<VwAgentesAsignados> VwAgentesAsignados()
        {
            var response = _context.VwAgentesAsignados.ToList();

            return response;
        }

        public NNAResponse ConsultarNNAsByTipoIdNumeroId(string tipoIdentificacionId, string numeroIdentificacion)
        {
            var response = new NNAResponse();

            try
            {
                var nnna = _context.NNAs.FirstOrDefault(x => x.TipoIdentificacionId == tipoIdentificacionId && x.NumeroIdentificacion == numeroIdentificacion);

                if (nnna != null)
                {
                    response = new NNAResponse()
                    {
                        NombreCompleto = string.Concat(nnna.PrimerNombre, " ", nnna.SegundoNombre, " ", nnna.PrimerApellido, " ", nnna.SegundoApellido),
                        Diagnostico = "",
                        FechaNacimiento = nnna.FechaNacimiento,
                        Id = nnna.Id
                    };
                }
                else
                {
                    response = null;
                }
            }
            catch (Exception)
            {
                response = null;
            }

            return response;
        }

        public RespuestaResponse<List<FiltroNNADto>> ConsultarNNAFiltro(FiltroNNARequest entrada)
        {
            var response = new RespuestaResponse<List<FiltroNNADto>>();
            response.Datos = new List<FiltroNNADto>();

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Estado", entrada.Estado ?? 0),
                    new SqlParameter("@Agente", entrada.Agente ?? ""),
                    new SqlParameter("@Buscar", entrada.Buscar ?? ""),
                    new SqlParameter("@Orden", entrada.Orden ?? 1)
                };

                var results = _context.FiltroNNAs.FromSqlRaw(
                    "EXEC dbo.SpConsultaNnaFiltro @Estado, @Agente, @Buscar, @Orden",
                    parameters
                ).ToList();

                if (results.Any())
                {
                    response.Estado = true;
                    response.Descripcion = "Consulta realizada con éxito.";
                }
                else
                {
                    response.Estado = false;
                    response.Descripcion = "No trajo datos en la consulta.";
                }

                foreach (var filtroNNA in results)
                {
                    FiltroNNADto dto = new()
                    {
                        NoCaso = filtroNNA.NoCaso,
                        NombreNNA = filtroNNA.NombreNNA,
                        NoDocumento = filtroNNA.NoDocumento,
                        UltimaActualizacion = filtroNNA.UltimaActualizacion,
                        AgenteAsignado = filtroNNA.AgenteAsignado,
                        EstadoId = filtroNNA.EstadoId,
                        Estado = filtroNNA.Estado,
                        EstadoDescripcion = filtroNNA.EstadoDescripcion,
                        EstadoColorBG = filtroNNA.EstadoColorBG,
                        EstadoColorText = filtroNNA.EstadoColorText
                    };

                    response.Datos.Add(dto);
                }


            }
            catch (Exception ex)
            {
                response.Estado = false;
                response.Descripcion = $"Error al realizar la consulta: {ex.Message}";
                response.Datos = null;
            }

            return response;
        }

        public void ActualizarNNASeguimiento(NNASeguimientoRequest request)
        {
            NNAs? nna = (from nn in _context.NNAs
                         where nn.Id == request.NNAId
                         select nn).FirstOrDefault();

            if (nna != null)
            {
                ContactoNNA? contacto;

                foreach (ContactoRequest c in request.Contactos)
                {
                    contacto = (from contact in _context.ContactoNNAs
                                where contact.Nombres == c.Nombre && contact.NNAId == request.NNAId
                                select contact).FirstOrDefault();

                    if (contacto != null)
                    {
                        contacto.ParentescoId = c.IdParentesco;
                        contacto.Telefonos = string.Concat(c.Telefono1, " ", c.Telefono2);

                        _context.ContactoNNAs.Update(contacto);
                    }
                    else
                    {
                        contacto = new ContactoNNA();
                        contacto.NNAId = request.NNAId;
                        contacto.Nombres = c.Nombre;
                        contacto.ParentescoId = c.IdParentesco;
                        contacto.Telefonos = string.Concat(c.Telefono1, " ", c.Telefono2);
                        _context.ContactoNNAs.Add(contacto);
                    }
                    _context.SaveChanges();
                }

                nna.OrigenReporteId = request.IdOrigenEstrategia;
                nna.PrimerNombre = request.PrimerNombreNNA;
                nna.SegundoNombre = request.SegundoNombreNNA;
                nna.PrimerApellido = request.PrimerApellidoNNA;
                nna.SegundoApellido = request.SegundoApellidoNNA;
                nna.TipoIdentificacionId = request.IdTipoIdentificacionNNA;
                nna.NumeroIdentificacion = request.NumeroIdentificacionNNA;
                nna.FechaNacimiento = request.FechaNacimientoNNA;
                nna.EtniaId = request.IdEtniaNNA;
                nna.GrupoPoblacionId = request.IdGrupoPoblacionalNNA;
                nna.SexoId = request.IdSexoNNA;
                nna.TipoRegimenSSId = request.IdRegimenAfiliacionNNA;
                nna.EAPBId = request.EAPBNNA;
                nna.OrigenReporteOtro = request.OtroOrigenEstrategia;
                nna.PaisId = request.IdPaisNacimientoNNA;

                _context.Update(nna);
                _context.SaveChanges();
            }
        }

        public NNAResponse ConsultarNNAsById(long NNAId)
        {
            var response = new NNAResponse();

            try
            {
                var nnna = (from nna in _context.NNAs
                            where nna.Id == NNAId
                            select new NNAResponse()
                            {
                                NombreCompleto = string.Concat(nna.PrimerNombre, " ", nna.SegundoNombre, " ", nna.PrimerApellido, " ", nna.SegundoApellido),
                                Diagnostico = "",
                                FechaNacimiento = nna.FechaNacimiento,
                                Id = nna.Id
                            }).FirstOrDefault();

                if (nnna != null)
                {
                    response = nnna;
                }
                else
                {
                    response = null;
                }
            }
            catch (Exception)
            {
                response = null;
            }

            return response;
        }

        public async Task<DatosBasicosNNAResponse>? ConsultarDatosBasicosNNAById(long NNAId, TablaParametricaService tablaParametricaService)
        {
            try
            {
                Seguimiento? seguimiento = await (from seg in _context.Seguimientos
                                                  where seg.NNAId == NNAId
                                                  orderby seg.Id descending
                                                  select seg).FirstOrDefaultAsync();

                DatosBasicosNNAResponse? response = await (from nna in _context.NNAs
                                                           where nna.Id == NNAId
                                                           select new DatosBasicosNNAResponse()
                                                           {
                                                               Diagnostico = "",
                                                               FechaInicioSegumiento = seguimiento.FechaSeguimiento,
                                                               FechaNacimiento = nna.FechaNacimiento,
                                                               NombreCompleto = string.Join("", nna.PrimerNombre, " ", nna.SegundoNombre, " ", nna.PrimerApellido, " ", nna.SegundoApellido),
                                                               DiagnosticoId = nna.DiagnosticoId
                                                           }).FirstOrDefaultAsync();

                if (response.DiagnosticoId != null)
                {
                    TPCIE10 cie10 = await _repositoryCie10.GetByIdAsync(response.DiagnosticoId.Value);

                    if (cie10 != null)
                    {
                        response.Diagnostico = cie10.Nombre;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<SolicitudSeguimientoCuidadorResponse> SolicitudSeguimientoCuidador(long NNAId, TablaParametricaService tablaParametricaService)
        {
            Seguimiento? seguimiento = await (from seg in _context.Seguimientos
                                              where seg.NNAId == NNAId
                                              orderby seg.Id descending
                                              select seg).FirstOrDefaultAsync();

            SolicitudSeguimientoCuidadorResponse? response = await (from nna in _context.NNAs
                                                                    where nna.Id == NNAId
                                                                    select new SolicitudSeguimientoCuidadorResponse()
                                                                    {
                                                                        Base64Adjunto = "",
                                                                        CorreoSolicitante = "",
                                                                        DiagnosticoNNA = "",
                                                                        FechaDiagnostico = nna.FechaDiagnostico,
                                                                        FechaNacimientoNNA = nna.FechaNacimiento,
                                                                        NoCaso = nna.Id,
                                                                        NombreAdjunto = "",
                                                                        NombreCompletoNNA = string.Join("", nna.PrimerNombre, " ", nna.SegundoNombre, " ", nna.PrimerApellido, " ", nna.SegundoApellido),
                                                                        NombreSolicitante = "",
                                                                        ObservacionSolicitante = seguimiento == null ? "" : seguimiento.ObservacionesSolicitante,
                                                                        SexoNNa = nna.SexoId.ToString(),
                                                                        TelefonoSolicitante = "",
                                                                        DiagnosticoId = nna.DiagnosticoId

                                                                    }).FirstOrDefaultAsync();

            List<TPExternalEntityBase> cie10 = await tablaParametricaService.GetBynomTREFCodigo("CIE10", response.DiagnosticoId, CancellationToken.None);

            if (cie10 != null && cie10.Count > 0)
            {
                response.DiagnosticoNNA = cie10[0].Nombre;
            }

            return response;
        }

        public DepuracionProtocoloResponse DepuracionProtocolo(List<DepuracionProtocoloRequest> request)
        {
            int nuevos = 0;
            int recaidas = 0;
            int segundaNeoplasia = 0;
            int duplicados = 0;
            int ingresados = 0;
            DepuracionProtocoloResponse response = new();
            try
            {
                Dictionary<string, List<DepuracionProtocolo>> dNNAProtocolo = new();

                List<DepuracionProtocolo> listaDepuracion = new();

                for (int i = 0; i < request.Count; i++)
                {
                    DepuracionProtocolo depuracion = new()
                    {
                        Id = i,
                        DepuracionProtocoloRequest = request[i]
                    };
                    listaDepuracion.Add(depuracion);
                }

                // eliminar registros con ajuste 6 o d
                for (int i = listaDepuracion.Count - 1; i >= 0; i--)
                {
                    if (listaDepuracion[i].DepuracionProtocoloRequest.ajuste == "6" || listaDepuracion[i].DepuracionProtocoloRequest.ajuste == "D")
                    {
                        listaDepuracion.RemoveAt(i);
                    }
                }

                foreach (DepuracionProtocolo d in listaDepuracion)
                {
                    //por documento
                    if (dNNAProtocolo.ContainsKey(d.DepuracionProtocoloRequest.tip_ide + "&" + d.DepuracionProtocoloRequest.num_ide))
                    {
                        dNNAProtocolo[d.DepuracionProtocoloRequest.tip_ide + "&" + d.DepuracionProtocoloRequest.num_ide].Add(d);
                    }
                    else
                    {
                        dNNAProtocolo.Add(d.DepuracionProtocoloRequest.tip_ide + "&" + d.DepuracionProtocoloRequest.num_ide, new List<DepuracionProtocolo>());
                        dNNAProtocolo[d.DepuracionProtocoloRequest.tip_ide + "&" + d.DepuracionProtocoloRequest.num_ide].Add(d);
                    }

                    //por nombre 
                    if (dNNAProtocolo.ContainsKey(d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape))
                    {
                        dNNAProtocolo[d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape].Add(d);
                    }
                    else
                    {
                        dNNAProtocolo.Add(d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape, new List<DepuracionProtocolo>());
                        dNNAProtocolo[d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape].Add(d);
                    }

                    //por nombre 
                    if (dNNAProtocolo.ContainsKey(d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape))
                    {
                        dNNAProtocolo[d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape].Add(d);
                    }
                    else
                    {
                        dNNAProtocolo.Add(d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape, new List<DepuracionProtocolo>());
                        dNNAProtocolo[d.DepuracionProtocoloRequest.pri_nom + "&" + d.DepuracionProtocoloRequest.seg_nom + "&" + d.DepuracionProtocoloRequest.pri_ape + "&" + d.DepuracionProtocoloRequest.seg_ape].Add(d);
                    }
                }

                bool fallecido;
                int recorridoCaso2 = 0;
                int recorridoCaso3 = 0;
                DateTime fechaNotificacion;
                Dictionary<int, List<DateTime?>> dTrazabilidad;
                int cantidadMaxima = 0;
                bool anioActual;
                List<DepuracionProtocolo> depuracionManual;
                List<DepuracionProtocolo> insertarNNa;


                foreach (KeyValuePair<string, List<DepuracionProtocolo>> kvp in dNNAProtocolo)
                {
                    if (kvp.Value.Count > 1)
                    {
                        duplicados += 1;
                    }
                    else
                    {
                        ingresados += 1;
                    }
                }

                foreach (KeyValuePair<string, List<DepuracionProtocolo>> kvp in dNNAProtocolo)
                {
                    if (kvp.Value.Count > 1)
                    {
                        //priorizar fallecidos
                        fallecido = false;
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.con_fin == "2")
                            {
                                fallecido = true;
                            }
                        }

                        if (fallecido)
                        {
                            for (int i = kvp.Value.Count - 1; i >= 0; i--)
                            {
                                if (kvp.Value[i].DepuracionProtocoloRequest.con_fin != "2")
                                {
                                    kvp.Value.RemoveAt(i);
                                }
                            }
                        }
                    }

                    if (kvp.Value.Count > 1)
                    {
                        //recorrido del caso
                        recorridoCaso2 = 0;
                        recorridoCaso3 = 0;
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.tip_cas == "2")
                            {
                                recorridoCaso2 = 2;
                            }
                            else if (kvp.Value[i].DepuracionProtocoloRequest.ajuste == "3")
                            {
                                recorridoCaso3 = 3;
                            }
                        }

                        if (recorridoCaso2 == 2)
                        {
                            for (int i = kvp.Value.Count - 1; i >= 0; i--)
                            {
                                if (kvp.Value[i].DepuracionProtocoloRequest.tip_cas != "2")
                                {
                                    kvp.Value.RemoveAt(i);
                                }
                            }
                        }
                        else if (recorridoCaso3 == 3)
                        {
                            for (int i = kvp.Value.Count - 1; i >= 0; i--)
                            {
                                if (kvp.Value[i].DepuracionProtocoloRequest.tip_cas != "3")
                                {
                                    kvp.Value.RemoveAt(i);
                                }
                            }
                        }

                    }

                    if (kvp.Value.Count > 1)
                    {
                        //caso oportuno
                        fechaNotificacion = DateTime.Now;
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_not < fechaNotificacion)
                            {
                                fechaNotificacion = kvp.Value[i].DepuracionProtocoloRequest.fec_not;
                            }
                        }

                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_not != fechaNotificacion)
                            {
                                kvp.Value.RemoveAt(i);
                            }
                        }
                    }

                    if (kvp.Value.Count > 1)
                    {
                        dTrazabilidad = new Dictionary<int, List<DateTime?>>();
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            List<DateTime?> key = new();
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_initra != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_initra);
                            }
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_tomadp != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_tomadp);
                            }
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_res_dp != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_res_dp);
                            }
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_tomadd != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_tomadd);
                            }
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_res_dd != null)
                            {
                                key.Add(kvp.Value[i].DepuracionProtocoloRequest.fec_res_dd);
                            }
                            List<DateTime?> value;
                            if (!dTrazabilidad.TryGetValue(kvp.Value[i].Id, out value))
                            {
                                dTrazabilidad.Add(kvp.Value[i].Id, key);
                            }

                        }

                        cantidadMaxima = 0;
                        foreach (KeyValuePair<int, List<DateTime?>> t in dTrazabilidad)
                        {
                            if (cantidadMaxima < t.Value.Count)
                            {
                                cantidadMaxima = t.Value.Count;
                            }
                        }

                        List<DateTime?> date = null;
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            dTrazabilidad.TryGetValue(kvp.Value[i].Id, out date);

                            if (date.Count < cantidadMaxima)
                            {
                                kvp.Value.RemoveAt(i);
                            }
                        }
                    }


                    if (kvp.Value.Count > 1)
                    {
                        // Inicializar los indicadores de tipo de cáncer
                        bool tieneCancer1 = false, tieneCancer2 = false, tieneCancer3 = false;
                        bool tieneCancer4 = false, tieneCancer13 = false, tieneCancer14 = false;

                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            var tipoCa = kvp.Value[i].DepuracionProtocoloRequest.tipo_ca;

                            switch (tipoCa)
                            {
                                case "1": tieneCancer1 = true; break;
                                case "2": tieneCancer2 = true; break;
                                case "3": tieneCancer3 = true; break;
                                case "4": tieneCancer4 = true; break;
                                case "13": tieneCancer13 = true; break;
                                case "14": tieneCancer14 = true; break;
                            }
                        }

                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            var tipoCa = kvp.Value[i].DepuracionProtocoloRequest.tipo_ca;

                            if ((tieneCancer1 && tipoCa != "1") ||
                                (tieneCancer2 && tipoCa != "2") ||
                                (tieneCancer3 && tipoCa != "3") ||
                                (tieneCancer4 && tipoCa != "4") ||
                                (tieneCancer13 && tipoCa != "13") ||
                                (tieneCancer14 && tipoCa != "14"))
                            {
                                kvp.Value.RemoveAt(i);
                            }
                        }
                    }

                    if (kvp.Value.Count > 1)
                    {
                        anioActual = false;

                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_not.Year == DateTime.Now.Year ||
                                kvp.Value[i].DepuracionProtocoloRequest.recaida == "1" ||
                                kvp.Value[i].DepuracionProtocoloRequest.consx2_neo == "1")
                            {
                                anioActual = true;
                            }
                        }

                        if (anioActual)
                        {
                            for (int i = kvp.Value.Count - 1; i >= 0; i--)
                            {
                                if (kvp.Value[i].DepuracionProtocoloRequest.fec_not.Year != DateTime.Now.Year)
                                {
                                    kvp.Value.RemoveAt(i);
                                }
                            }
                        }
                    }
                }

                depuracionManual = new List<DepuracionProtocolo>();
                insertarNNa = new List<DepuracionProtocolo>();
                foreach (KeyValuePair<string, List<DepuracionProtocolo>> kvp in dNNAProtocolo)
                {
                    if (kvp.Value.Count > 1)
                    {
                        depuracionManual.AddRange(kvp.Value);
                    }
                    else
                    {
                        insertarNNa.AddRange(kvp.Value);
                    }
                }

                List<NNAs> insertNNA = [];
                List<NNAs> updateNNA = [];
                //DateTime fechaDefuncion;
                foreach (DepuracionProtocolo d in insertarNNa)
                {
                    if (d.DepuracionProtocoloRequest.consx2_neo == "1")
                    {
                        segundaNeoplasia += 1;
                    }
                    else if (d.DepuracionProtocoloRequest.recaida == "1")
                    {
                        recaidas += 1;
                    }

                    var nna = (from nnas in _context.NNAs
                               where nnas.NumeroIdentificacion == d.DepuracionProtocoloRequest.num_ide
                               select nnas).FirstOrDefault();

                    if (nna != null)
                    {
                        if (d.DepuracionProtocoloRequest.consx2_neo == "1")
                        {
                            nna.TipoCancerId = d.DepuracionProtocoloRequest.tipo_ca;
                            if (DateTime.TryParse(d.DepuracionProtocoloRequest.fec_def, out DateTime fechaDefuncion))
                            {
                                nna.FechaDefuncion = fechaDefuncion;
                            }
                            nna.MotivoDefuncion = d.DepuracionProtocoloRequest.cbmte;
                            updateNNA.Add(nna);
                        }
                        else if (d.DepuracionProtocoloRequest.recaida == "1")
                        {
                            nna.Recaida = true;
                            nna.TipoCancerId = d.DepuracionProtocoloRequest.tipo_ca;
                            if (DateTime.TryParse(d.DepuracionProtocoloRequest.fec_def, out DateTime fechaDefuncion))
                            {
                                nna.FechaDefuncion = fechaDefuncion;
                            }
                            nna.MotivoDefuncion = d.DepuracionProtocoloRequest.cbmte;
                            updateNNA.Add(nna);
                        }
                        else
                        {
                            depuracionManual.Add(d);
                        }
                    }
                    else
                    {
                        nuevos += 1;
                        DateTime.TryParse(d.DepuracionProtocoloRequest.fec_def, out DateTime fechaDefuncion);
                        nna = new NNAs()
                        {
                            DateCreated = DateTime.Now,
                            CreatedByUserId = "1",
                            FechaNotificacionSIVIGILA = d.DepuracionProtocoloRequest.fec_not,
                            EPSId = 0,//verificar de donde sale el id de la eps
                            PrimerNombre = d.DepuracionProtocoloRequest.pri_nom,
                            SegundoNombre = d.DepuracionProtocoloRequest.seg_nom,
                            PrimerApellido = d.DepuracionProtocoloRequest.pri_ape,
                            SegundoApellido = d.DepuracionProtocoloRequest.seg_ape,
                            TipoIdentificacionId = d.DepuracionProtocoloRequest.tip_ide,
                            NumeroIdentificacion = d.DepuracionProtocoloRequest.num_ide,
                            SexoId = d.DepuracionProtocoloRequest.sexo,
                            PaisId = d.DepuracionProtocoloRequest.cod_pais_r,
                            ResidenciaOrigenMunicipioId = d.DepuracionProtocoloRequest.cod_mun_r,
                            ResidenciaOrigenAreaId = d.DepuracionProtocoloRequest.area,
                            ResidenciaOrigenBarrio = d.DepuracionProtocoloRequest.bar_ver,
                            ResidenciaOrigenDireccion = d.DepuracionProtocoloRequest.dir_res,
                            TipoRegimenSSId = d.DepuracionProtocoloRequest.tip_ss,
                            EtniaId = d.DepuracionProtocoloRequest.per_etn,
                            ResidenciaOrigenEstratoId = d.DepuracionProtocoloRequest.estrato,
                            GrupoPoblacionId = "", //verificar como se asocia a un grupo poblacional
                            FechaConsultaDiagnostico = d.DepuracionProtocoloRequest.fec_con,
                            FechaInicioSintomas = d.DepuracionProtocoloRequest.ini_sin,
                            FechaHospitalizacion = d.DepuracionProtocoloRequest.fec_hos,
                            FechaDefuncion = fechaDefuncion == DateTime.MinValue ? null : fechaDefuncion,
                            ResidenciaOrigenTelefono = d.DepuracionProtocoloRequest.telefono,
                            FechaNacimiento = d.DepuracionProtocoloRequest.fecha_nto,
                            MotivoDefuncion = d.DepuracionProtocoloRequest.cbmte,
                            TipoCancerId = d.DepuracionProtocoloRequest.tipo_ca,
                            FechaInicioTratamiento = d.DepuracionProtocoloRequest.fec_initra,
                            Recaida = d.DepuracionProtocoloRequest.recaida == "1" ? true : false,
                            FechaDiagnostico = d.DepuracionProtocoloRequest.fec_diag1a,
                            CuidadorTelefono = d.DepuracionProtocoloRequest.tel_cont_2,
                        };
                        insertNNA.Add(nna);
                    }
                }

                _context.NNAs.UpdateRange(updateNNA);

                _context.NNAs.AddRange(insertNNA);

                List<DepuracionManualProtocolo> depuracionProtocolos = new();

                foreach (DepuracionProtocolo d in depuracionManual)
                {
                    DepuracionManualProtocolo dep = new()
                    {
                        ajuste = d.DepuracionProtocoloRequest.ajuste,
                        anio = d.DepuracionProtocoloRequest.anio,
                        area = d.DepuracionProtocoloRequest.area,
                        bar_ver = d.DepuracionProtocoloRequest.bar_ver,
                        cbmte = d.DepuracionProtocoloRequest.cbmte,
                        cen_pobla = d.DepuracionProtocoloRequest.cen_pobla,
                        cer_def = d.DepuracionProtocoloRequest.cer_def,
                        cod_ase = d.DepuracionProtocoloRequest.cod_ase,
                        cod_dpto_o = d.DepuracionProtocoloRequest.cod_dpto_o,
                        cod_dpto_r = d.DepuracionProtocoloRequest.cod_dpto_r,
                        cod_eve = d.DepuracionProtocoloRequest.cod_eve,
                        cod_mun_o = d.DepuracionProtocoloRequest.cod_mun_o,
                        cod_mun_r = d.DepuracionProtocoloRequest.cod_mun_r,
                        cod_pais_o = d.DepuracionProtocoloRequest.cod_pais_o,
                        cod_pais_r = d.DepuracionProtocoloRequest.cod_pais_r,
                        cod_pre = d.DepuracionProtocoloRequest.cod_pre,
                        cod_sub = d.DepuracionProtocoloRequest.cod_sub,
                        consx2_neo = d.DepuracionProtocoloRequest.consx2_neo,
                        con_fin = d.DepuracionProtocoloRequest.con_fin,
                        crit_dx_de = d.DepuracionProtocoloRequest.crit_dx_de,
                        crit_dx_pr = d.DepuracionProtocoloRequest.crit_dx_pr,
                        dir_res = d.DepuracionProtocoloRequest.dir_res,
                        edad = d.DepuracionProtocoloRequest.edad,
                        estrato = d.DepuracionProtocoloRequest.estrato,
                        estrato_datos_complementarios = d.DepuracionProtocoloRequest.estrato_datos_complementarios,
                        FechaHora = d.DepuracionProtocoloRequest.FechaHora,
                        fecha_nto = d.DepuracionProtocoloRequest.fecha_nto,
                        fec_aju = d.DepuracionProtocoloRequest.fec_aju,
                        fec_arc_xl = d.DepuracionProtocoloRequest.fec_arc_xl,
                        fec_con = d.DepuracionProtocoloRequest.fec_con,
                        fec_def = d.DepuracionProtocoloRequest.fec_def,
                        fec_diag1a = d.DepuracionProtocoloRequest.fec_diag1a,
                        fec_hos = d.DepuracionProtocoloRequest.fec_hos,
                        fec_res_dd = d.DepuracionProtocoloRequest.fec_res_dd,
                        fec_initra = d.DepuracionProtocoloRequest.fec_initra,
                        fec_not = d.DepuracionProtocoloRequest.fec_not,
                        fec_res_dp = d.DepuracionProtocoloRequest.fec_res_dp,
                        fec_tomadd = d.DepuracionProtocoloRequest.fec_tomadd,
                        fec_tomadp = d.DepuracionProtocoloRequest.fec_tomadp,
                        fm_fuerza = d.DepuracionProtocoloRequest.fm_fuerza,
                        fm_grado = d.DepuracionProtocoloRequest.fm_grado,
                        fm_unidad = d.DepuracionProtocoloRequest.fm_unidad,
                        fuente = d.DepuracionProtocoloRequest.fuente,
                        gp_carcela = d.DepuracionProtocoloRequest.gp_carcela,
                        gp_desmovi = d.DepuracionProtocoloRequest.gp_desmovi,
                        gp_desplaz = d.DepuracionProtocoloRequest.gp_desplaz,
                        gp_discapa = d.DepuracionProtocoloRequest.gp_discapa,
                        gp_gestan = d.DepuracionProtocoloRequest.gp_gestan,
                        gp_indigen = d.DepuracionProtocoloRequest.gp_indigen,
                        gp_mad_com = d.DepuracionProtocoloRequest.gp_mad_com,
                        gp_migrant = d.DepuracionProtocoloRequest.gp_migrant,
                        gp_otros = d.DepuracionProtocoloRequest.gp_otros,
                        gp_pobicbf = d.DepuracionProtocoloRequest.gp_pobicbf,
                        gp_psiquia = d.DepuracionProtocoloRequest.gp_psiquia,
                        gp_vic_vio = d.DepuracionProtocoloRequest.gp_vic_vio,
                        ini_sin = d.DepuracionProtocoloRequest.ini_sin,
                        localidad = d.DepuracionProtocoloRequest.localidad,
                        nacionali = d.DepuracionProtocoloRequest.nacionali,
                        ndep_notif = d.DepuracionProtocoloRequest.ndep_notif,
                        ndep_proce = d.DepuracionProtocoloRequest.ndep_proce,
                        ndep_resi = d.DepuracionProtocoloRequest.ndep_resi,
                        nit_upgd = d.DepuracionProtocoloRequest.nit_upgd,
                        nmun_notif = d.DepuracionProtocoloRequest.nmun_notif,
                        nmun_proce = d.DepuracionProtocoloRequest.nmun_proce,
                        nmun_resi = d.DepuracionProtocoloRequest.nmun_resi,
                        nombre_nacionalidad = d.DepuracionProtocoloRequest.nombre_nacionalidad,
                        nom_dil_f = d.DepuracionProtocoloRequest.nom_dil_f,
                        nom_eve = d.DepuracionProtocoloRequest.nom_eve,
                        nom_grupo = d.DepuracionProtocoloRequest.nom_grupo,
                        nom_oncolo = d.DepuracionProtocoloRequest.nom_oncolo,
                        nom_upgd = d.DepuracionProtocoloRequest.nom_upgd,
                        npais_proce = d.DepuracionProtocoloRequest.npais_proce,
                        npais_resi = d.DepuracionProtocoloRequest.npais_proce,
                        num_ide = d.DepuracionProtocoloRequest.num_ide,
                        nuni_modif = d.DepuracionProtocoloRequest.nuni_modif,
                        ocupacion = d.DepuracionProtocoloRequest.ocupacion,
                        pac_hos = d.DepuracionProtocoloRequest.pac_hos,
                        per_etn = d.DepuracionProtocoloRequest.per_etn,
                        pri_ape = d.DepuracionProtocoloRequest.pri_ape,
                        pri_nom = d.DepuracionProtocoloRequest.pri_nom,
                        recaida = d.DepuracionProtocoloRequest.recaida,
                        seg_ape = d.DepuracionProtocoloRequest.seg_ape,
                        seg_nom = d.DepuracionProtocoloRequest.seg_nom,
                        semana = d.DepuracionProtocoloRequest.semana,
                        sem_ges = d.DepuracionProtocoloRequest.sem_ges,
                        sexo = d.DepuracionProtocoloRequest.sexo,
                        telefono = d.DepuracionProtocoloRequest.telefono,
                        tel_cont_2 = d.DepuracionProtocoloRequest.tel_cont_2,
                        tel_dil_f = d.DepuracionProtocoloRequest.tel_dil_f,
                        tel_oncolo = d.DepuracionProtocoloRequest.tel_oncolo,
                        tipo_ca = d.DepuracionProtocoloRequest.tipo_ca,
                        tip_cas = d.DepuracionProtocoloRequest.tip_cas,
                        tip_ide = d.DepuracionProtocoloRequest.tip_ide,
                        tip_ss = d.DepuracionProtocoloRequest.tip_ss,
                        uni_med = d.DepuracionProtocoloRequest.uni_med,
                        uni_modif = d.DepuracionProtocoloRequest.uni_modif,
                        vereda = d.DepuracionProtocoloRequest.vereda,
                        version = d.DepuracionProtocoloRequest.version,
                    };
                    depuracionProtocolos.Add(dep);
                }

                _context.DepuracionManualProtocolo.AddRange(depuracionProtocolos);
                _context.SaveChanges();

                ReporteDepuracion reporte = new()
                {
                    Estado = "Procesada",
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Hora = TimeOnly.FromDateTime(DateTime.Now),
                    Recaidas = recaidas,
                    RegistrosDuplicados = duplicados,
                    RegistrosIngresados = ingresados,
                    RegistrosNuevos = nuevos,
                    SegundasNeoplasias = segundaNeoplasia
                };

                _context.ReporteDepuracion.AddRange(reporte);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                ReporteDepuracion reporte = new()
                {
                    Estado = "Procesada",
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Hora = TimeOnly.FromDateTime(DateTime.Now),
                    Recaidas = recaidas,
                    RegistrosDuplicados = duplicados,
                    RegistrosIngresados = ingresados,
                    RegistrosNuevos = nuevos,
                    SegundasNeoplasias = segundaNeoplasia
                };

                _context.ReporteDepuracion.AddRange(reporte);
                _context.SaveChanges();
            }

            if (nuevos == 0)
                return new() { Estado = "Procesada" };
            else
                return new() { Estado = "Procesada", Nuevos = nuevos, Recaidas = recaidas, SegundasNeoplasias = segundaNeoplasia };
        }

        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            NNAs? nna = null;
            if (seguimiento != null)
            {
                nna = (from nn in _context.NNAs
                       where nn.Id == seguimiento.NNAId
                       select nn).FirstOrDefault();
            }

            if (nna != null)
            {
                nna.ResidenciaOrigenMunicipioId = request.residenciaOrigen.IdMunicipio;
                nna.ResidenciaOrigenBarrio = request.residenciaOrigen.Barrio;
                nna.ResidenciaOrigenAreaId = request.residenciaOrigen.IdArea;
                nna.ResidenciaOrigenDireccion = request.residenciaOrigen.Direccion;
                nna.ResidenciaOrigenEstratoId = request.residenciaOrigen.IdEstrato;
                nna.ResidenciaOrigenTelefono = request.residenciaOrigen.TelefonoFijo;

                if (request.residenciaDestino != null)
                {
                    nna.ResidenciaActualMunicipioId = request.residenciaDestino.IdMunicipio;
                    nna.ResidenciaActualBarrio = request.residenciaDestino.Barrio;
                    nna.ResidenciaActualAreaId = request.residenciaDestino.IdArea;
                    nna.ResidenciaActualDireccion = request.residenciaDestino.Direccion;
                    nna.ResidenciaActualEstratoId = request.residenciaDestino.IdEstrato;
                    nna.ResidenciaActualTelefono = request.residenciaDestino.TelefonoFijo;
                }

                nna.TrasladoTieneCapacidadEconomica = request.CapacidadEconomicaTraslado;
                nna.TrasladoEAPBSuministroApoyo = request.ServiciosSocialesEAPB;
                nna.TrasladosServiciosdeApoyoOportunos = request.ServiciosSocialesEntregados;
                nna.TrasladosServiciosdeApoyoCobertura = request.ServiciosSocialesCobertura;
                nna.TrasladosHaSolicitadoApoyoFundacion = request.ApoyoRecibidoFundacion;
                nna.TrasladosNombreFundacion = request.NombreFundacion;
                nna.TrasladosPropietarioResidenciaActualId = request.IdTipoResidenciaActual;
                nna.TrasladosQuienAsumioCostosTraslado = request.AsumeCostoTraslado;
                nna.TrasladosQuienAsumioCostosVivienda = request.AsumeCostoVivienda;
                _context.NNAs.Update(nna);
                _context.SaveChanges();
            }
        }

        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            if (seguimiento != null)
            {
                NNAs? nna = (from nn in _context.NNAs
                             where nn.Id == seguimiento.NNAId
                             select nn).FirstOrDefault();

                if (nna != null)
                {

                    nna.DiagnosticoId = request.IdDiagnostico;
                    nna.FechaConsultaDiagnostico = request.FechaDiagnostico;
                    nna.FechaConsultaOrigenReporte = request.FechaConsulta;
                    nna.FechaInicioTratamiento = request.FechaInicioTratamiento;
                    nna.IPSId = request.IdIPS;
                    nna.Recaida = request.Recaidas;
                    nna.CantidadRecaidas = request.NumeroRecaidas;
                    nna.FechaUltimaRecaida = request.FechaUltimaRecaida;
                    nna.MotivoNoDiagnosticoId = request.IdMotivoNoDiagnostico;
                    nna.MotivoNoDiagnosticoOtro = request.RazonNoDiagnostico;
                }
            }
        }

        public void SetDificultadesProceso(DificultadesProcesoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            if (seguimiento != null)
            {
                NNAs? nna = (from nn in _context.NNAs
                             where nn.Id == seguimiento.NNAId
                             select nn).FirstOrDefault();

                if (nna != null)
                {
                    nna.DifAutorizaciondeMedicamentos = request.AutorizacionMedicamento;
                    nna.DifEntregaMedicamentosLAP = request.EntregaMedicamentoLAP;
                    nna.DifEntregaMedicamentosNoLAP = request.EntregaMedicamentoNoLAP;
                    nna.DifAsignaciondeCitas = request.AsignacionCitas;
                    nna.DifHanCobradoCuotasoCopagos = request.CobradoCopagos;
                    nna.DifAutorizacionProcedimientos = request.AutorizacionProcedimientos;
                    nna.DifRemisionInstitucionesEspecializadas = request.RemisionEspecialistas;
                    nna.DifMalaAtencionIPS = request.MalaAtencionIPS;
                    nna.DifMalaAtencionNombreIPSId = request.IdMalaIPS;
                    nna.DifFallasenMIPRES = request.FallasMIPRES;
                    nna.DifFallaConvenioEAPBeIPSTratante = request.FallasConvenio;
                    nna.CategoriaAlertaId = request.IdCategoriaAlerta;
                    nna.SubcategoriaAlertaId = request.IdSubcategoriaAlerta;
                    nna.TrasladosHaSidoTrasladadodeInstitucion = request.HaSidoTrasladado;
                    nna.TrasladosNumerodeTraslados = request.NumeroTraslados;
                    nna.TrasladosHaRecurridoAccionLegal = request.AccionLegal;
                    nna.TrasladosTipoAccionLegalId = request.IdTipoRecurso;
                    nna.TrasladosMotivoAccionLegal = request.MotivoAccionLegal;
                }
            }
        }

        public void SetAdherenciaProceso(AdherenciaProcesoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            if (seguimiento != null)
            {
                NNAs? nna = (from nn in _context.NNAs
                             where nn.Id == seguimiento.NNAId
                             select nn).FirstOrDefault();

                if (nna != null)
                {
                    nna.TratamientoHaDejadodeAsistir = request.HaDejadoTratamiento;
                    nna.TratamientoCuantoTiemposinAsistir = request.TiempoDejadoTratamiento;
                    nna.TratamientoUnidadMedidaIdTiempoId = request.IdUnidadTiempoDejadoTratamiento;
                    nna.TratamientoCausasInasistenciaId = request.IdCausaInasistenciaTratamiento;
                    nna.TratamientoCausasInasistenciaOtra = request.OtraCausaDejadoTratamiento;
                    nna.TratamientoEstudiaActualmente = request.EstudiaActualmente;
                    nna.TratamientoHaDejadodeAsistirColegio = request.HaDejadoEstudiar;
                    nna.TratamientoTiempoInasistenciaColegio = request.CuantoTiempoDejadoEstudiar;

                }
            }
        }

        public List<ConsultaCasosAbiertosResponse> ConsultaCasosAbiertos(CasosAbiertosRequest request)
        {
            List<ConsultaCasosAbiertosResponse> response = new();
            List<ConsultaCasosAbiertosResponse> lista = new();
            List<int> estados = new() { 2, 3, 4, 5, 6, 7, 8, 9, 15, 16 };

            lista = (from seg in _context.Seguimientos
                     join nna in _context.NNAs on seg.NNAId equals nna.Id
                     where nna.estadoId.HasValue && estados.Contains(nna.estadoId.Value)
                     select new ConsultaCasosAbiertosResponse()
                     {
                         AsuntoUltimaActuacion = seg.UltimaActuacionAsunto,
                         Estado = seg.EstadoId,
                         FechaNotificacion = seg.FechaSolicitud ?? new(),
                         FechaUltimaActuacion = seg.UltimaActuacionFecha,
                         Alertas = new List<AlertaSeguimientoResponse>(),
                         SeguimientoId = seg.Id
                     }).ToList();

            foreach (ConsultaCasosAbiertosResponse r in lista)
            {
                List<AlertaSeguimientoResponse> alertas = (from al in _context.AlertaSeguimientos
                                                           join alerta in _context.Alertas on al.AlertaId equals alerta.Id
                                                           where al.SeguimientoId == r.SeguimientoId
                                                           select new AlertaSeguimientoResponse()
                                                           {
                                                               AlertaId = al.AlertaId,
                                                               EstadoId = al.EstadoId,
                                                               NombreAlerta = alerta.Descripcion,
                                                               Observaciones = al.Observaciones,
                                                               SeguimientoId = al.SeguimientoId,
                                                               UltimaFechaSeguimiento = (DateTime)al.UltimaFechaSeguimiento
                                                           }).ToList();

                r.Alertas = alertas;
            }

            if (request.Filtro == "HOY")
            {
                response = (from re in lista
                            where re.FechaUltimaActuacion == DateTime.Now.Date
                            select re).ToList();
            }
            else if (request.Filtro == "ALERTA")
            {
                foreach (ConsultaCasosAbiertosResponse r in lista)
                {
                    if (r.Alertas != null && r.Alertas.Any())
                    {
                        response.Add(r);
                    }
                }
            }

            return response;

        }

        public void AsignacionManual(AsignacionManualRequest request)
        {
            UsuarioAsignado usuarioAsignado;
            List<UsuarioAsignado> usuarios = new();
            foreach (int i in request.Segumientos)
            {
                usuarioAsignado = new UsuarioAsignado()
                {
                    Activo = true,
                    DateCreated = DateTime.Now,
                    FechaAsignacion = DateTime.Now,
                    Observaciones = request.Motivo,
                    SeguimientoId = i,
                    UsuarioId = request.IdUsuario,
                };
                usuarios.Add(usuarioAsignado);
            }

            _context.UsuarioAsignados.AddRange(usuarios);
            _context.SaveChanges();
        }

        public DepuracionProtocoloResponse CargarArchivoNNA(IFormFile file)
        {
            List<DepuracionProtocoloRequest> DepuracionRequest = new();
            DepuracionProtocoloResponse response;
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Archivo no proporcionado o está vacío.");
            }

            var data = new List<List<string>>();

            // Lee el archivo Excel desde el IFormFile

            try
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyToAsync(stream);
                    string[] fileName = file.FileName.Split('.');
                    if (fileName[1].ToLower() == "xls" || fileName[1].ToLower() == "xlsx")
                    {
                        using (var workbook = new XLWorkbook(stream))
                        {
                            var worksheet = workbook.Worksheet(1); // Selecciona la primera hoja
                            var firstRow = worksheet.FirstRowUsed();
                            var lastRow = worksheet.LastRowUsed();

                            var headers = new List<string>();

                            // Leer encabezados (primera fila)
                            foreach (var cell in firstRow.CellsUsed())
                            {
                                headers.Add(cell.GetString());
                            }

                            // Recorrer las filas restantes
                            for (int row = firstRow.RowNumber() + 1; row <= lastRow.RowNumber(); row++)
                            {
                                DepuracionProtocoloRequest depuracion = new();
                                var rowData = new Dictionary<string, string>();
                                var currentRow = worksheet.Row(row);

                                depuracion.cod_eve = currentRow.Cell(1).GetValue<string>();
                                DateTime fec_not = DateTime.MinValue;
                                DateTime.TryParse(currentRow.Cell(2).GetValue<string>(), out fec_not);
                                if (fec_not != DateTime.MinValue)
                                {
                                    depuracion.fec_not = fec_not;
                                }

                                depuracion.semana = currentRow.Cell(3).GetValue<int>();
                                depuracion.anio = currentRow.Cell(4).GetValue<int>();
                                depuracion.cod_pre = currentRow.Cell(5).GetValue<string>();
                                depuracion.cod_sub = currentRow.Cell(6).GetValue<string>();
                                depuracion.pri_nom = currentRow.Cell(7).GetValue<string>();
                                depuracion.seg_nom = currentRow.Cell(8).GetValue<string>();
                                depuracion.pri_ape = currentRow.Cell(9).GetValue<string>();
                                depuracion.seg_ape = currentRow.Cell(10).GetValue<string>();
                                depuracion.tip_ide = currentRow.Cell(11).GetValue<string>();
                                depuracion.num_ide = currentRow.Cell(12).GetValue<string>();
                                depuracion.edad = currentRow.Cell(13).GetValue<int>();
                                depuracion.uni_med = currentRow.Cell(14).GetValue<string>();
                                depuracion.nacionali = currentRow.Cell(15).GetValue<string>();
                                depuracion.nombre_nacionalidad = currentRow.Cell(16).GetValue<string>();
                                depuracion.sexo = currentRow.Cell(17).GetValue<string>();
                                depuracion.cod_pais_o = currentRow.Cell(18).GetValue<string>();
                                depuracion.cod_dpto_o = currentRow.Cell(19).GetValue<string>();
                                depuracion.cod_mun_o = currentRow.Cell(20).GetValue<string>();
                                depuracion.area = currentRow.Cell(21).GetValue<string>();
                                depuracion.localidad = currentRow.Cell(22).GetValue<string>();
                                depuracion.cen_pobla = currentRow.Cell(23).GetValue<string>();
                                depuracion.vereda = currentRow.Cell(24).GetValue<string>();
                                depuracion.bar_ver = currentRow.Cell(25).GetValue<string>();
                                depuracion.dir_res = currentRow.Cell(26).GetValue<string>();
                                depuracion.ocupacion = currentRow.Cell(27).GetValue<string>();
                                depuracion.tip_ss = currentRow.Cell(28).GetValue<string>();
                                depuracion.cod_ase = currentRow.Cell(29).GetValue<string>();
                                depuracion.per_etn = currentRow.Cell(30).GetValue<string>();
                                depuracion.nom_grupo = currentRow.Cell(31).GetValue<string>();
                                depuracion.estrato = currentRow.Cell(32).GetValue<string>();
                                depuracion.gp_discapa = currentRow.Cell(33).GetValue<string>();
                                depuracion.gp_desplaz = currentRow.Cell(34).GetValue<string>();
                                depuracion.gp_migrant = currentRow.Cell(35).GetValue<string>();
                                depuracion.gp_carcela = currentRow.Cell(36).GetValue<string>();
                                depuracion.gp_gestan = currentRow.Cell(37).GetValue<string>();
                                depuracion.sem_ges = currentRow.Cell(38).GetValue<string>();
                                depuracion.gp_indigen = currentRow.Cell(39).GetValue<string>();
                                depuracion.gp_pobicbf = currentRow.Cell(40).GetValue<string>();
                                depuracion.gp_mad_com = currentRow.Cell(41).GetValue<string>();
                                depuracion.gp_desmovi = currentRow.Cell(42).GetValue<string>();
                                depuracion.gp_psiquia = currentRow.Cell(43).GetValue<string>();
                                depuracion.gp_vic_vio = currentRow.Cell(44).GetValue<string>();
                                depuracion.gp_otros = currentRow.Cell(45).GetValue<string>();
                                depuracion.fuente = currentRow.Cell(46).GetValue<string>();
                                depuracion.cod_pais_r = currentRow.Cell(47).GetValue<string>();
                                depuracion.cod_dpto_r = currentRow.Cell(48).GetValue<string>();
                                depuracion.cod_mun_r = currentRow.Cell(49).GetValue<string>();
                                DateTime fec_con = DateTime.MinValue;
                                DateTime.TryParse(currentRow.Cell(50).GetValue<string>(), out fec_con);
                                if (fec_not != DateTime.MinValue)
                                {
                                    depuracion.fec_con = fec_con;
                                }
                                DateTime ini_sin = DateTime.MinValue;
                                DateTime.TryParse(currentRow.Cell(51).GetValue<string>(), out ini_sin);
                                if (ini_sin != DateTime.MinValue)
                                {
                                    depuracion.ini_sin = ini_sin;
                                }
                                depuracion.tip_cas = currentRow.Cell(52).GetValue<string>();
                                depuracion.pac_hos = currentRow.Cell(53).GetValue<string>();
                                DateTime fec_hos = DateTime.MinValue;
                                DateTime.TryParse(currentRow.Cell(54).GetValue<string>(), out fec_hos);
                                if (fec_hos != DateTime.MinValue)
                                {
                                    depuracion.fec_hos = fec_hos;
                                }
                                depuracion.con_fin = currentRow.Cell(55).GetValue<String>();
                                depuracion.fec_def = currentRow.Cell(56).GetValue<string>();
                                depuracion.ajuste = currentRow.Cell(57).GetValue<string>();
                                depuracion.telefono = currentRow.Cell(58).GetValue<string>();
                                DateTime fecha_nto = DateTime.MinValue;
                                DateTime.TryParse(currentRow.Cell(59).GetValue<string>(), out fecha_nto);
                                if (fecha_nto != DateTime.MinValue)
                                {
                                    depuracion.fecha_nto = fecha_nto;
                                }
                                depuracion.cer_def = currentRow.Cell(60).GetValue<string>();
                                depuracion.cbmte = currentRow.Cell(61).GetValue<string>();
                                depuracion.uni_modif = currentRow.Cell(62).GetValue<string>();
                                depuracion.nuni_modif = currentRow.Cell(63).GetValue<string>();
                                DateTime fec_arc_xl = DateTime.MinValue;
                                DateTime.TryParse(currentRow.Cell(64).GetValue<string>(), out fec_arc_xl);
                                if (fec_arc_xl != DateTime.MinValue)
                                {
                                    depuracion.fec_arc_xl = fec_arc_xl;
                                }
                                depuracion.nom_dil_f = currentRow.Cell(65).GetValue<string>();
                                depuracion.tel_dil_f = currentRow.Cell(66).GetValue<string>();
                                DateTime fec_aju = DateTime.MinValue;
                                DateTime.TryParse(currentRow.Cell(67).GetValue<string>(), out fec_aju);
                                if (fec_aju != DateTime.MinValue)
                                {
                                    depuracion.fec_aju = fec_aju;
                                }
                                depuracion.nit_upgd = currentRow.Cell(68).GetValue<string>();
                                depuracion.fm_fuerza = currentRow.Cell(69).GetValue<string>();
                                depuracion.fm_unidad = currentRow.Cell(70).GetValue<string>();
                                depuracion.fm_grado = currentRow.Cell(71).GetValue<string>();
                                depuracion.version = currentRow.Cell(72).GetValue<string>();
                                depuracion.tipo_ca = currentRow.Cell(73).GetValue<string>();
                                DateTime fec_initra;
                                DateTime.TryParse(currentRow.Cell(74).GetValue<string>(), out fec_initra);
                                if (fec_initra != DateTime.MinValue)
                                {
                                    depuracion.fec_initra = fec_initra;
                                }
                                depuracion.consx2_neo = currentRow.Cell(75).GetValue<string>();
                                depuracion.recaida = currentRow.Cell(76).GetValue<string>();
                                DateTime fec_diag1a;
                                DateTime.TryParse(currentRow.Cell(77).GetValue<string>(), out fec_diag1a);
                                if (fec_diag1a != DateTime.MinValue)
                                {
                                    depuracion.fec_diag1a = fec_diag1a;
                                }
                                depuracion.crit_dx_pr = currentRow.Cell(78).GetValue<string>();
                                DateTime fec_tomadp;
                                DateTime.TryParse(currentRow.Cell(79).GetValue<string>(), out fec_tomadp);
                                if (fec_tomadp != DateTime.MinValue)
                                {
                                    depuracion.fec_tomadp = fec_tomadp;
                                }
                                DateTime fec_res_dp;
                                DateTime.TryParse(currentRow.Cell(80).GetValue<string>(), out fec_res_dp);
                                if (fec_res_dp != DateTime.MinValue)
                                {
                                    depuracion.fec_res_dp = fec_res_dp;
                                }
                                depuracion.crit_dx_de = currentRow.Cell(81).GetValue<string>();
                                DateTime fec_tomadd;
                                DateTime.TryParse(currentRow.Cell(82).GetValue<string>(), out fec_tomadd);
                                if (fec_tomadd != DateTime.MinValue)
                                {
                                    depuracion.fec_tomadd = fec_tomadd;
                                }
                                DateTime.TryParse(currentRow.Cell(83).GetValue<string>(), out DateTime fec_res_dd);
                                if (fec_tomadd != DateTime.MinValue)
                                {
                                    depuracion.fec_res_dd = fec_res_dd;
                                }
                                depuracion.nom_oncolo = currentRow.Cell(84).GetValue<string>();
                                depuracion.tel_oncolo = currentRow.Cell(85).GetValue<string>();
                                depuracion.tel_cont_2 = currentRow.Cell(86).GetValue<string>();
                                depuracion.estrato_datos_complementarios = currentRow.Cell(87).GetValue<string>();
                                depuracion.nom_eve = currentRow.Cell(88).GetValue<string>();
                                depuracion.nom_upgd = currentRow.Cell(89).GetValue<string>();
                                depuracion.npais_proce = currentRow.Cell(90).GetValue<string>();
                                depuracion.ndep_proce = currentRow.Cell(91).GetValue<string>();
                                depuracion.nmun_proce = currentRow.Cell(92).GetValue<string>();
                                depuracion.npais_proce = currentRow.Cell(93).GetValue<string>();
                                depuracion.ndep_resi = currentRow.Cell(94).GetValue<string>();
                                depuracion.nmun_resi = currentRow.Cell(95).GetValue<string>();
                                depuracion.ndep_notif = currentRow.Cell(96).GetValue<string>();
                                depuracion.nmun_notif = currentRow.Cell(97).GetValue<string>();
                                DateTime FechaHora;
                                DateTime.TryParse(currentRow.Cell(98).GetValue<string>(), out FechaHora);
                                if (FechaHora != DateTime.MinValue)
                                {
                                    depuracion.FechaHora = FechaHora;
                                }

                                DepuracionRequest.Add(depuracion);
                            }
                        }
                    }
                    else if (fileName[1].ToLower() == "csv")
                    {
                        var registros = new List<DepuracionProtocoloRequest>();

                        // Configurar el stream para leer desde el principio
                        stream.Seek(0, SeekOrigin.Begin);

                        using var reader = new StreamReader(stream);
                        string? line;
                        int lineNumber = 0;


                        while ((line = reader.ReadLine()) != null)
                        {
                            // Saltar encabezado
                            if (lineNumber == 0)
                            {
                                lineNumber++;
                                continue;
                            }

                            // Dividir la línea en columnas usando coma como separador
                            var columns = line.Split(',');
                            DepuracionProtocoloRequest depuracion = new();
                            // Crear un objeto con las posiciones respectivas
                            depuracion.cod_eve = columns[0];
                            DateTime fec_not = DateTime.MinValue;
                            DateTime.TryParse(columns[1], out fec_not);
                            if (fec_not != DateTime.MinValue)
                            {
                                depuracion.fec_not = fec_not;
                            }

                            depuracion.semana = int.Parse(columns[2]);
                            depuracion.anio = int.Parse(columns[3]);
                            depuracion.cod_pre = columns[4];
                            depuracion.cod_sub = columns[5];
                            depuracion.pri_nom = columns[6];
                            depuracion.seg_nom = columns[7];
                            depuracion.pri_ape = columns[8];
                            depuracion.seg_ape = columns[9];
                            depuracion.tip_ide = columns[10];
                            depuracion.num_ide = columns[11];
                            depuracion.edad = Int32.Parse(columns[12]);
                            depuracion.uni_med = columns[13];
                            depuracion.nacionali = columns[14];
                            depuracion.nombre_nacionalidad = columns[15];
                            depuracion.sexo = columns[16];
                            depuracion.cod_pais_o = columns[17];
                            depuracion.cod_dpto_o = columns[18];
                            depuracion.cod_mun_o = columns[19];
                            depuracion.area = columns[20];
                            depuracion.localidad = columns[21];
                            depuracion.cen_pobla = columns[22];
                            depuracion.vereda = columns[23];
                            depuracion.bar_ver = columns[24];
                            depuracion.dir_res = columns[25];
                            depuracion.ocupacion = columns[26];
                            depuracion.tip_ss = columns[27];
                            depuracion.cod_ase = columns[28];
                            depuracion.per_etn = columns[29];
                            depuracion.nom_grupo = columns[30];
                            depuracion.estrato = columns[31];
                            depuracion.gp_discapa = columns[32];
                            depuracion.gp_desplaz = columns[33];
                            depuracion.gp_migrant = columns[34];
                            depuracion.gp_carcela = columns[35];
                            depuracion.gp_gestan = columns[36];
                            depuracion.sem_ges = columns[37];
                            depuracion.gp_indigen = columns[38];
                            depuracion.gp_pobicbf = columns[39];
                            depuracion.gp_mad_com = columns[40];
                            depuracion.gp_desmovi = columns[41];
                            depuracion.gp_psiquia = columns[42];
                            depuracion.gp_vic_vio = columns[43];
                            depuracion.gp_otros = columns[44];
                            depuracion.fuente = columns[45];
                            depuracion.cod_pais_r = columns[46];
                            depuracion.cod_dpto_r = columns[47];
                            depuracion.cod_mun_r = columns[48];
                            DateTime fec_con = DateTime.MinValue;
                            DateTime.TryParse(columns[49], out fec_con);
                            if (fec_not != DateTime.MinValue)
                            {
                                depuracion.fec_con = fec_con;
                            }
                            DateTime ini_sin = DateTime.MinValue;
                            try
                            {
                                DateTime.TryParse(columns[50], out ini_sin);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.StackTrace);
                            }

                            if (ini_sin != DateTime.MinValue)
                            {
                                depuracion.ini_sin = ini_sin;
                            }
                            depuracion.tip_cas = columns[51];
                            depuracion.pac_hos = columns[52];
                            DateTime fec_hos = DateTime.MinValue;
                            DateTime.TryParse(columns[53], out fec_hos);
                            if (fec_hos != DateTime.MinValue)
                            {
                                depuracion.fec_hos = fec_hos;
                            }
                            depuracion.con_fin = columns[54];
                            depuracion.fec_def = columns[55];
                            depuracion.ajuste = columns[56];
                            depuracion.telefono = columns[57];
                            DateTime fecha_nto = DateTime.MinValue;
                            DateTime.TryParse(columns[58], out fecha_nto);
                            if (fecha_nto != DateTime.MinValue)
                            {
                                depuracion.fecha_nto = fecha_nto;
                            }
                            depuracion.cer_def = columns[59];
                            depuracion.cbmte = columns[60];
                            depuracion.uni_modif = columns[61];
                            depuracion.nuni_modif = columns[62];
                            DateTime fec_arc_xl = DateTime.MinValue;
                            try
                            {
                                DateTime.TryParse(columns[63], out fec_arc_xl);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.StackTrace);
                            }

                            if (fec_arc_xl != DateTime.MinValue)
                            {
                                depuracion.fec_arc_xl = fec_arc_xl;
                            }
                            depuracion.nom_dil_f = columns[64];
                            depuracion.tel_dil_f = columns[65];
                            DateTime fec_aju = DateTime.MinValue;
                            DateTime.TryParse(columns[66], out fec_aju);
                            if (fec_aju != DateTime.MinValue)
                            {
                                depuracion.fec_aju = fec_aju;
                            }
                            depuracion.nit_upgd = columns[67];
                            depuracion.fm_fuerza = columns[68];
                            depuracion.fm_unidad = columns[69];
                            depuracion.fm_grado = columns[70];
                            depuracion.version = columns[71];
                            depuracion.tipo_ca = columns[72];
                            DateTime fec_initra;
                            DateTime.TryParse(columns[73], out fec_initra);
                            if (fec_initra != DateTime.MinValue)
                            {
                                depuracion.fec_initra = fec_initra;
                            }
                            depuracion.consx2_neo = columns[74];
                            depuracion.recaida = columns[75];
                            DateTime fec_diag1a;
                            DateTime.TryParse(columns[76], out fec_diag1a);
                            if (fec_diag1a != DateTime.MinValue)
                            {
                                depuracion.fec_diag1a = fec_diag1a;
                            }
                            depuracion.crit_dx_pr = columns[77];
                            DateTime fec_tomadp;
                            DateTime.TryParse(columns[78], out fec_tomadp);
                            if (fec_tomadp != DateTime.MinValue)
                            {
                                depuracion.fec_tomadp = fec_tomadp;
                            }
                            DateTime fec_res_dp;
                            DateTime.TryParse(columns[79], out fec_res_dp);
                            if (fec_res_dp != DateTime.MinValue)
                            {
                                depuracion.fec_res_dp = fec_res_dp;
                            }
                            depuracion.crit_dx_de = columns[80];
                            DateTime fec_tomadd;
                            DateTime.TryParse(columns[81], out fec_tomadd);
                            if (fec_tomadd != DateTime.MinValue)
                            {
                                depuracion.fec_tomadd = fec_tomadd;
                            }
                            DateTime fec_res_dd;
                            DateTime.TryParse(columns[82], out fec_res_dd);
                            if (fec_tomadd != DateTime.MinValue)
                            {
                                depuracion.fec_res_dd = fec_res_dd;
                            }
                            depuracion.nom_oncolo = columns[83];
                            depuracion.tel_oncolo = columns[84];
                            depuracion.tel_cont_2 = columns[85];
                            depuracion.estrato_datos_complementarios = columns[86];
                            depuracion.nom_eve = columns[87];
                            depuracion.nom_upgd = columns[88];
                            depuracion.npais_proce = columns[89];
                            depuracion.ndep_proce = columns[90];
                            depuracion.nmun_proce = columns[91];
                            depuracion.npais_proce = columns[92];
                            depuracion.ndep_resi = columns[93];
                            depuracion.nmun_resi = columns[94];
                            depuracion.ndep_notif = columns[95];
                            depuracion.nmun_notif = columns[96];
                            DateTime FechaHora;
                            DateTime.TryParse(columns[97], out FechaHora);
                            if (FechaHora != DateTime.MinValue)
                            {
                                depuracion.FechaHora = FechaHora;
                            }

                            DepuracionRequest.Add(depuracion);
                        }
                    }

                }
                response = this.DepuracionProtocolo(DepuracionRequest);
            }
            catch (Exception ex)
            {
                response = new DepuracionProtocoloResponse()
                {
                    Estado = $"Ocurrió un error al procesar el archivo: {ex.Message}"
                };
            }

            return response;
        }
    }
}