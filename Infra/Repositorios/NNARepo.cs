using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Infra.Repositories.Common;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace Infra.Repositorios
{
    public class NNARepo : INNARepo
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<NNAs> _repository;


        public NNARepo(ApplicationDbContext context)
        {
            _context = context;
            GenericRepository<NNAs> repository = new(_context);
            _repository = repository;
        }

        public async Task<NNADto?> GetById(long id)
        {
            try
            {
                var result = await _repository.GetByIdAsync(id) ?? throw new Exception("Entity not found");
                return result.Adapt<NNADto>();
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
                throw new Exception("cannot add entity");
            }
            return (success, response);
        }

        public async Task<(bool, NNAs)> UpdateAsync(NNAs entity)
        {
            var (success, response) = await _repository.UpdateAsync(entity);
            if (!success)
            {
                throw new Exception("cannot update entity");
            }
            return (success, response);
        }

        public List<VwAgentesAsignados> VwAgentesAsignados()
        {
            var response = _context.VwAgentesAsignados.ToList();

            return response;
        }

        public NNAs ConsultarNNAsByTipoIdNumeroId(string tipoIdentificacionId, string numeroIdentificacion)
        {
            var response = new NNAs();

            try
            {
                var nnna = _context.NNAs.FirstOrDefault(x => x.TipoIdentificacionId == tipoIdentificacionId && x.NumeroIdentificacion == numeroIdentificacion);

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

        public RespuestaResponse<FiltroNNADto> ConsultarNNAFiltro(FiltroNNARequest entrada)
        {
            var response = new RespuestaResponse<FiltroNNADto>();
            response.Datos = new List<FiltroNNADto>();
            List<FiltroNNA> lista = new();

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
                if (results != null)
                {
                    if (results.Count() > 0)
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
                else
                {
                    response.Estado = true;
                    response.Descripcion = "No trae información.";
                    response.Datos = null;
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



        public NNAs ConsultarNNAsById(long NNAId)
        {
            var response = new NNAs();

            try
            {
                var nnna = (from nna in _context.NNAs
                            where nna.Id == NNAId
                            select nna).FirstOrDefault();

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

            List<TPExternalEntityBase> cie10 = await tablaParametricaService.GetBynomTREFCodigo("CIE10", response.DiagnosticoId, CancellationToken.None);

            if (cie10 != null && cie10.Count > 0)
            {
                response.Diagnostico = cie10[0].Nombre;
            }

            return response;
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
            DepuracionProtocoloResponse response = new DepuracionProtocoloResponse();
            try
            {
                Dictionary<string, List<DepuracionProtocolo>> dNNAProtocolo = new Dictionary<string, List<DepuracionProtocolo>>();

                List<DepuracionProtocolo> listaDepuracion = new List<DepuracionProtocolo>();

                for (int i = 0; i < request.Count; i++)
                {
                    DepuracionProtocolo depuracion = new DepuracionProtocolo()
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
                bool tieneCancer1;
                bool tieneCancer2;
                bool tieneCancer3;
                bool tieneCancer4;
                bool tieneCancer13;
                bool tieneCancer14;
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
                            List<DateTime?> key = new List<DateTime?>();
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
                            dTrazabilidad.Add(kvp.Value[i].Id, key);
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
                        //tipoCancer
                        tieneCancer1 = false;
                        tieneCancer2 = false;
                        tieneCancer3 = false;
                        tieneCancer4 = false;
                        tieneCancer13 = false;
                        tieneCancer14 = false;
                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (kvp.Value[i].DepuracionProtocoloRequest.tipo_ca == "1")
                            {
                                tieneCancer1 = true;
                            }
                            else if (kvp.Value[i].DepuracionProtocoloRequest.tipo_ca == "2")
                            {
                                tieneCancer2 = true;
                            }
                            else if (kvp.Value[i].DepuracionProtocoloRequest.tipo_ca == "3")
                            {
                                tieneCancer3 = true;
                            }
                            else if (kvp.Value[i].DepuracionProtocoloRequest.tipo_ca == "4")
                            {
                                tieneCancer4 = true;
                            }
                            else if (kvp.Value[i].DepuracionProtocoloRequest.tipo_ca == "13")
                            {
                                tieneCancer4 = true;
                            }
                            else if (kvp.Value[i].DepuracionProtocoloRequest.tipo_ca == "14")
                            {
                                tieneCancer14 = true;
                            }
                        }

                        for (int i = kvp.Value.Count - 1; i >= 0; i--)
                        {
                            if (tieneCancer1 && kvp.Value[i].DepuracionProtocoloRequest.tipo_ca != "1")
                            {
                                kvp.Value.RemoveAt(i);
                            }
                            else if (tieneCancer2 && kvp.Value[i].DepuracionProtocoloRequest.tipo_ca != "2")
                            {
                                kvp.Value.RemoveAt(i);
                            }
                            else if (tieneCancer3 && kvp.Value[i].DepuracionProtocoloRequest.tipo_ca != "3")
                            {
                                kvp.Value.RemoveAt(i);
                            }
                            else if (tieneCancer4 && kvp.Value[i].DepuracionProtocoloRequest.tipo_ca != "4")
                            {
                                kvp.Value.RemoveAt(i);
                            }
                            else if (tieneCancer13 && kvp.Value[i].DepuracionProtocoloRequest.tipo_ca != "13")
                            {
                                kvp.Value.RemoveAt(i);
                            }
                            else if (tieneCancer14 && kvp.Value[i].DepuracionProtocoloRequest.tipo_ca != "14")
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
                            if (kvp.Value[i].DepuracionProtocoloRequest.fec_not.Year == DateTime.Now.Year)
                            {
                                anioActual = true;
                            }
                            else if (kvp.Value[i].DepuracionProtocoloRequest.recaida == "1")
                            {
                                anioActual = true;
                            }
                            else if (kvp.Value[i].DepuracionProtocoloRequest.consx2_neo == "1")
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

                List<NNAs> insertNNA = new List<NNAs>();
                List<NNAs> updateNNA = new List<NNAs>();
                DateTime fechaDefuncion;
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
                    else
                    {
                        nuevos += 1;
                    }

                    NNAs? nna = (from nnas in _context.NNAs
                                 where nnas.NumeroIdentificacion == d.DepuracionProtocoloRequest.num_ide
                                 select nnas).FirstOrDefault();

                    if (nna != null)
                    {
                        if (d.DepuracionProtocoloRequest.consx2_neo == "1")
                        {
                            nna.TipoCancerId = d.DepuracionProtocoloRequest.tipo_ca;
                            if (DateTime.TryParse(d.DepuracionProtocoloRequest.fec_def, out fechaDefuncion))
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
                            if (DateTime.TryParse(d.DepuracionProtocoloRequest.fec_def, out fechaDefuncion))
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
                        fechaDefuncion = DateTime.MinValue;
                        DateTime.TryParse(d.DepuracionProtocoloRequest.fec_def, out fechaDefuncion);
                        nna = new NNAs()
                        {
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
                            FechaDefuncion = (fechaDefuncion == DateTime.MinValue ? null : fechaDefuncion),
                            ResidenciaOrigenTelefono = d.DepuracionProtocoloRequest.telefono,
                            FechaNacimiento = d.DepuracionProtocoloRequest.fecha_nto,
                            MotivoDefuncion = d.DepuracionProtocoloRequest.cbmte,
                            TipoCancerId = d.DepuracionProtocoloRequest.tipo_ca,
                            FechaInicioTratamiento = d.DepuracionProtocoloRequest.fec_initra,
                            Recaida = (d.DepuracionProtocoloRequest.recaida == "1" ? true : false),
                            FechaDiagnostico = d.DepuracionProtocoloRequest.fec_diag1a,
                            CuidadorTelefono = d.DepuracionProtocoloRequest.tel_cont_2,
                        };
                        insertNNA.Add(nna);
                    }
                }

                _context.NNAs.UpdateRange(updateNNA);

                _context.NNAs.AddRange(insertNNA);

                List<DepuracionManualProtocolo> depuracionProtocolos = new List<DepuracionManualProtocolo>();

                foreach (DepuracionProtocolo d in depuracionManual)
                {
                    DepuracionManualProtocolo dep = new DepuracionManualProtocolo()
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

                _context.DepuracionManualProtocolos.AddRange(depuracionProtocolos);
                _context.SaveChanges();

                ReporteDepuracion reporte = new ReporteDepuracion()
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

                _context.ReporteDepuracions.AddRange(reporte);
                _context.SaveChanges();

                response.Nuevos = nuevos;
                response.Recaidas = recaidas;
                response.SegundasNeoplasias = segundaNeoplasia;
                response.Estado = reporte.Estado;
            }
            catch (Exception ex)
            {
                ReporteDepuracion reporte = new ReporteDepuracion()
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

                _context.ReporteDepuracions.AddRange(reporte);
                _context.SaveChanges();

                response.Nuevos = nuevos;
                response.Recaidas = recaidas;
                response.SegundasNeoplasias = segundaNeoplasia;
                response.Estado = reporte.Estado;
            }

            return response;
        }
    }

}