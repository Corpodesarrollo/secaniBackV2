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
            DepuracionProtocoloResponse response = new();

            List<DepuracionProtocolo> listaDepuracion = new();

            for (int i = 0; i < request.Count; i++)
            {
                DepuracionProtocolo depuracion = new()
                {
                    Id = i,
                    DepuracionProtocoloRequest = request[i]
                };

            }


            Dictionary<string, List<DepuracionProtocolo>> DTipoNumeroDocumento = new();
            Dictionary<string, List<DepuracionProtocolo>> DNombreTipoCancer = new();
            Dictionary<string, List<DepuracionProtocolo>> DNombreFechaNotificacion = new();
            Dictionary<string, List<DepuracionProtocolo>> DFallecido = new();
            Dictionary<string, List<DepuracionProtocolo>> DTrazabilidad = new();

            Dictionary<string, List<DepuracionProtocolo>> DfechaDefuncion = new();

            foreach (DepuracionProtocolo r in listaDepuracion)
            {
                //mismo tipo y numero de identificacion
                if (DTipoNumeroDocumento.ContainsKey(r.DepuracionProtocoloRequest.tip_ide + " " + r.DepuracionProtocoloRequest.num_ide))
                {
                    DTipoNumeroDocumento[r.DepuracionProtocoloRequest.tip_ide + " " + r.DepuracionProtocoloRequest.num_ide].Add(r);
                }
                else
                {
                    DTipoNumeroDocumento.Add(r.DepuracionProtocoloRequest.tip_ide + " " + r.DepuracionProtocoloRequest.num_ide, new List<DepuracionProtocolo>());
                    DTipoNumeroDocumento[r.DepuracionProtocoloRequest.tip_ide + " " + r.DepuracionProtocoloRequest.num_ide].Add(r);
                }

                //mismo nombre y tipo de cancer 
                if (DNombreTipoCancer.ContainsKey(r.DepuracionProtocoloRequest.pri_nom + " " + r.DepuracionProtocoloRequest.seg_nom + " " + r.DepuracionProtocoloRequest.pri_ape + " " + r.DepuracionProtocoloRequest.seg_ape + " "/*falta la columna de tipo de cancer*/))
                {
                    DNombreTipoCancer[r.DepuracionProtocoloRequest.pri_nom + " " + r.DepuracionProtocoloRequest.seg_nom + " " + r.DepuracionProtocoloRequest.pri_ape + " " + r.DepuracionProtocoloRequest.seg_ape].Add(r);
                }
                else
                {
                    DNombreTipoCancer.Add(r.DepuracionProtocoloRequest.pri_nom + " " + r.DepuracionProtocoloRequest.seg_nom + " " + r.DepuracionProtocoloRequest.pri_ape + " " + r.DepuracionProtocoloRequest.seg_ape, new List<DepuracionProtocolo>());
                    DNombreTipoCancer[r.DepuracionProtocoloRequest.pri_nom + " " + r.DepuracionProtocoloRequest.seg_nom + " " + r.DepuracionProtocoloRequest.pri_ape + " " + r.DepuracionProtocoloRequest.seg_ape].Add(r);
                }

                //mismo nombre y fecha de notificacion
                if (DNombreFechaNotificacion.ContainsKey(r.DepuracionProtocoloRequest.pri_nom + " " + r.DepuracionProtocoloRequest.seg_nom + " " + r.DepuracionProtocoloRequest.pri_ape + " " + r.DepuracionProtocoloRequest.seg_ape + " " + r.DepuracionProtocoloRequest.fec_not))
                {
                    DNombreFechaNotificacion[r.DepuracionProtocoloRequest.pri_nom + " " + r.DepuracionProtocoloRequest.seg_nom + " " + r.DepuracionProtocoloRequest.pri_ape + " " + r.DepuracionProtocoloRequest.seg_ape + " " + r.DepuracionProtocoloRequest.fec_not].Add(r);
                }
                else
                {
                    DNombreFechaNotificacion.Add(r.DepuracionProtocoloRequest.pri_nom + " " + r.DepuracionProtocoloRequest.seg_nom + " " + r.DepuracionProtocoloRequest.pri_ape + " " + r.DepuracionProtocoloRequest.seg_ape + " " + r.DepuracionProtocoloRequest.fec_not, new List<DepuracionProtocolo>());
                    DNombreFechaNotificacion[r.DepuracionProtocoloRequest.pri_nom + " " + r.DepuracionProtocoloRequest.seg_nom + " " + r.DepuracionProtocoloRequest.pri_ape + " " + r.DepuracionProtocoloRequest.seg_ape + " " + r.DepuracionProtocoloRequest.fec_not].Add(r);
                }
            }

            //identificacion de fallecidos
            DFallecido = this.IdentificacionFallecidos(DTipoNumeroDocumento, DNombreTipoCancer, DNombreFechaNotificacion);

            //falta recorrido del caso

            //mayor trazabilidad
            DTrazabilidad = IdentificacionMayorTrazabilidad(DTipoNumeroDocumento, DNombreTipoCancer, DNombreFechaNotificacion, DFallecido);

            //falta verificacion del tipo de cancer

            //falta casos de años anteriores

            //falta casos con diferentes ipos de cancer

            //falta eliminacion de registros    

            //caso mas oportuno es el ultimo

            return response;
        }

        private Dictionary<string, List<DepuracionProtocolo>> IdentificacionFallecidos(Dictionary<string, List<DepuracionProtocolo>> DTipoNumeroDocumento,
            Dictionary<string, List<DepuracionProtocolo>> DNombreTipoCancer, Dictionary<string, List<DepuracionProtocolo>> DNombreFechaNotificacion)
        {
            Dictionary<string, List<DepuracionProtocolo>> DFallecido = new();

            foreach (KeyValuePair<string, List<DepuracionProtocolo>> par in DTipoNumeroDocumento)
            {
                foreach (DepuracionProtocolo r in par.Value)
                {
                    if (r.DepuracionProtocoloRequest.fec_def != null &&
                        r.DepuracionProtocoloRequest.fec_def.Trim() != "-   -")
                    {
                        if (DFallecido.ContainsKey(par.Key))
                        {
                            DFallecido[par.Key].Add(r);
                        }
                        else
                        {
                            DFallecido.Add(par.Key, new List<DepuracionProtocolo>());
                            DFallecido[par.Key].Add(r);
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, List<DepuracionProtocolo>> par in DNombreTipoCancer)
            {
                foreach (DepuracionProtocolo r in par.Value)
                {
                    if (r.DepuracionProtocoloRequest.fec_def != null &&
                        r.DepuracionProtocoloRequest.fec_def.Trim() != "-   -")
                    {
                        if (DFallecido.ContainsKey(par.Key))
                        {
                            DFallecido[par.Key].Add(r);
                        }
                        else
                        {
                            DFallecido.Add(par.Key, new List<DepuracionProtocolo>());
                            DFallecido[par.Key].Add(r);
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, List<DepuracionProtocolo>> par in DNombreFechaNotificacion)
            {
                foreach (DepuracionProtocolo r in par.Value)
                {
                    if (r.DepuracionProtocoloRequest.fec_def != null &&
                        r.DepuracionProtocoloRequest.fec_def.Trim() != "-   -")
                    {
                        if (DFallecido.ContainsKey(par.Key))
                        {
                            DFallecido[par.Key].Add(r);
                        }
                        else
                        {
                            DFallecido.Add(par.Key, new List<DepuracionProtocolo>());
                            DFallecido[par.Key].Add(r);
                        }
                    }
                }
            }

            return DFallecido;
        }

        private Dictionary<string, List<DepuracionProtocolo>> IdentificacionMayorTrazabilidad(Dictionary<string, List<DepuracionProtocolo>> DTipoNumeroDocumento,
            Dictionary<string, List<DepuracionProtocolo>> DNombreTipoCancer, Dictionary<string, List<DepuracionProtocolo>> DNombreFechaNotificacion,
            Dictionary<string, List<DepuracionProtocolo>> DFallecido)
        {
            Dictionary<string, List<DepuracionProtocolo>> DTrazabilidad = new();
            foreach (KeyValuePair<string, List<DepuracionProtocolo>> par in DTipoNumeroDocumento)
            {
                if (!DFallecido.ContainsKey(par.Key))
                {
                    foreach (DepuracionProtocolo r in par.Value)
                    {
                        if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null && r.DepuracionProtocoloRequest.fec_res_dp != null
                            && r.DepuracionProtocoloRequest.fec_tomadd != null && r.DepuracionProtocoloRequest.fec_res_dd != null)
                        {
                            if (DTrazabilidad.ContainsKey(par.Key))
                            {
                                DTrazabilidad[par.Key].Add(r);
                            }
                            else
                            {
                                DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                                DTrazabilidad[par.Key].Add(r);
                            }
                        }
                        else if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null && r.DepuracionProtocoloRequest.fec_res_dp != null
                            && r.DepuracionProtocoloRequest.fec_tomadd != null)
                        {
                            if (DTrazabilidad.TryGetValue(par.Key, out List<DepuracionProtocolo>? value))
                            {
                                value.Add(r);
                            }
                            else
                            {
                                DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                                DTrazabilidad[par.Key].Add(r);
                            }
                        }
                        else if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null && r.DepuracionProtocoloRequest.fec_res_dp != null)
                        {
                            if (DTrazabilidad.ContainsKey(par.Key))
                            {
                                DTrazabilidad[par.Key].Add(r);
                            }
                            else
                            {
                                DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                                DTrazabilidad[par.Key].Add(r);
                            }
                        }
                        else if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null)
                        {
                            if (DTrazabilidad.ContainsKey(par.Key))
                            {
                                DTrazabilidad[par.Key].Add(r);
                            }
                            else
                            {
                                DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                                DTrazabilidad[par.Key].Add(r);
                            }
                        }
                        else if (r.DepuracionProtocoloRequest.fec_initra != null)
                        {
                            if (DTrazabilidad.ContainsKey(par.Key))
                            {
                                DTrazabilidad[par.Key].Add(r);
                            }
                            else
                            {
                                DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                                DTrazabilidad[par.Key].Add(r);
                            }
                        }
                    }
                }

            }

            foreach (KeyValuePair<string, List<DepuracionProtocolo>> par in DNombreTipoCancer)
            {
                foreach (DepuracionProtocolo r in par.Value)
                {
                    if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null && r.DepuracionProtocoloRequest.fec_res_dp != null
                           && r.DepuracionProtocoloRequest.fec_tomadd != null && r.DepuracionProtocoloRequest.fec_res_dd != null)
                    {
                        if (DTrazabilidad.ContainsKey(par.Key))
                        {
                            DTrazabilidad[par.Key].Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                    else if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null && r.DepuracionProtocoloRequest.fec_res_dp != null
                        && r.DepuracionProtocoloRequest.fec_tomadd != null)
                    {
                        if (DTrazabilidad.TryGetValue(par.Key, out List<DepuracionProtocolo>? value))
                        {
                            value.Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                    else if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null && r.DepuracionProtocoloRequest.fec_res_dp != null)
                    {
                        if (DTrazabilidad.ContainsKey(par.Key))
                        {
                            DTrazabilidad[par.Key].Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                    else if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null)
                    {
                        if (DTrazabilidad.ContainsKey(par.Key))
                        {
                            DTrazabilidad[par.Key].Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                    else if (r.DepuracionProtocoloRequest.fec_initra != null)
                    {
                        if (DTrazabilidad.ContainsKey(par.Key))
                        {
                            DTrazabilidad[par.Key].Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, List<DepuracionProtocolo>> par in DNombreFechaNotificacion)
            {
                foreach (DepuracionProtocolo r in par.Value)
                {
                    if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null && r.DepuracionProtocoloRequest.fec_res_dp != null
                            && r.DepuracionProtocoloRequest.fec_tomadd != null && r.DepuracionProtocoloRequest.fec_res_dd != null)
                    {
                        if (DTrazabilidad.ContainsKey(par.Key))
                        {
                            DTrazabilidad[par.Key].Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                    else if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null && r.DepuracionProtocoloRequest.fec_res_dp != null
                        && r.DepuracionProtocoloRequest.fec_tomadd != null)
                    {
                        if (DTrazabilidad.TryGetValue(par.Key, out List<DepuracionProtocolo>? value))
                        {
                            value.Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                    else if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null && r.DepuracionProtocoloRequest.fec_res_dp != null)
                    {
                        if (DTrazabilidad.ContainsKey(par.Key))
                        {
                            DTrazabilidad[par.Key].Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                    else if (r.DepuracionProtocoloRequest.fec_initra != null && r.DepuracionProtocoloRequest.fec_tomadp != null)
                    {
                        if (DTrazabilidad.ContainsKey(par.Key))
                        {
                            DTrazabilidad[par.Key].Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                    else if (r.DepuracionProtocoloRequest.fec_initra != null)
                    {
                        if (DTrazabilidad.ContainsKey(par.Key))
                        {
                            DTrazabilidad[par.Key].Add(r);
                        }
                        else
                        {
                            DTrazabilidad.Add(par.Key, new List<DepuracionProtocolo>());
                            DTrazabilidad[par.Key].Add(r);
                        }
                    }
                }
            }

            return DTrazabilidad;
        }
    }

}