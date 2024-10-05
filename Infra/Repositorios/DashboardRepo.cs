using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;
using Core.Modelos;
using MSAuthentication.Core.DTOs;
using Core.Response;
using System.Runtime.Intrinsics.X86;


namespace Infra.Repositorios
{
    public class DashboardRepo : IDashboardRepo
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepo(ApplicationDbContext context) => _context = context;

        public GetTotalDashboardResponse RepoDashboardTotalCasos(DateTime FechaInicial, DateTime FechaFinal)
        {
            DateTime startDatePreviousWeek = FechaInicial.AddDays(-7);
            DateTime endDatePreviousWeek = FechaFinal.AddDays(-7);

            // Obtenemos el conteo de casos actuales y anteriores directamente
            var totalCasosActual = _context.NNAs
                .Where(s => s.FechaIngresoEstrategia >= FechaInicial && s.FechaIngresoEstrategia <= FechaFinal)
                .Count();

            var totalCasosAnterior = _context.NNAs
                .Where(s => s.FechaIngresoEstrategia >= startDatePreviousWeek && s.FechaIngresoEstrategia <= endDatePreviousWeek)
                .Count();

            var totalCasosGeneral = _context.NNAs
               .Count();

            // Retornamos un solo objeto de respuesta
            return new GetTotalDashboardResponse
            {
                TotalCasosGeneral = totalCasosGeneral,
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }

        public GetTotalDashboardResponse RepoDashboardTotalRegistros(DateTime FechaInicial, DateTime FechaFinal, string? EntidadId)
        {
           
            DateTime startDatePreviousWeek = FechaInicial.AddDays(-7);
            DateTime endDatePreviousWeek = FechaFinal.AddDays(-7);

            // Obtenemos el conteo de casos actuales directamente
            var totalCasosActual = _context.NNAs
                .Where(s => s.DateCreated >= FechaInicial && s.DateCreated <= FechaFinal)
                .Count();

            // Obtenemos el conteo de casos de la semana anterior
            var totalCasosAnterior = _context.NNAs
                .Where(s => s.DateCreated >= startDatePreviousWeek && s.DateCreated <= endDatePreviousWeek)
                .Count();

            var totalCasosGeneral = _context.NNAs
                .Count();

            // Retornamos un único resultado
            return new GetTotalDashboardResponse
            {
                TotalCasosGeneral = totalCasosGeneral,
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }


        public GetTotalDashboardResponse RepoDashboardMisCasos(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {
            
            DateTime FechaInicialSemanaAnterior = FechaInicial.AddDays(-7);
            DateTime FechaFinalSemanaAnterior = FechaFinal.AddDays(-7);


            // Obtenemos el conteo de casos actuales directamente
            var totalCasosActual = (from ua in _context.UsuarioAsignados
                              where (string.IsNullOrEmpty(UsuarioID) || ua.UsuarioId == UsuarioID)
                                 && ua.FechaAsignacion >= FechaInicial
                                 && ua.FechaAsignacion <= FechaFinal
                              join s1 in
                                  (from s1 in _context.Seguimientos
                                   join s2 in
                                       (from s in _context.Seguimientos
                                        group s by s.NNAId into g
                                        select new { NNAId = g.Key, Id = g.Max(x => x.Id) })
                                   on s1.Id equals s2.Id
                                   select s1)
                              on ua.SeguimientoId equals s1.Id
                              select ua).Count();

            var totalCasosAnterior = (from ua in _context.UsuarioAsignados
                                    where (string.IsNullOrEmpty(UsuarioID) || ua.UsuarioId == UsuarioID)
                                       && ua.FechaAsignacion >= FechaInicialSemanaAnterior
                                       && ua.FechaAsignacion <= FechaFinalSemanaAnterior
                                      join s1 in
                                        (from s1 in _context.Seguimientos
                                         join s2 in
                                             (from s in _context.Seguimientos
                                              group s by s.NNAId into g
                                              select new { NNAId = g.Key, Id = g.Max(x => x.Id) })
                                         on s1.Id equals s2.Id
                                         select s1)
                                    on ua.SeguimientoId equals s1.Id
                                    select ua).Count();

            var totalCasosGeneral = (from ua in _context.UsuarioAsignados
                                    where (string.IsNullOrEmpty(UsuarioID) || ua.UsuarioId == UsuarioID)

                                     join s1 in
                                        (from s1 in _context.Seguimientos
                                         join s2 in
                                             (from s in _context.Seguimientos
                                              group s by s.NNAId into g
                                              select new { NNAId = g.Key, Id = g.Max(x => x.Id) })
                                         on s1.Id equals s2.Id
                                         select s1)
                                    on ua.SeguimientoId equals s1.Id
                                    select ua).Count();


            // Retornamos un único resultado
            return new GetTotalDashboardResponse
            {
                TotalCasosGeneral = totalCasosGeneral,
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }


        public GetTotalDashboardResponse RepoDashboardAlertas(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {
           
            DateTime FechaInicialSemanaAnterior = FechaInicial.AddDays(-7);
            DateTime FechaFinalSemanaAnterior = FechaFinal.AddDays(-7);


            // Calcular alertas actuales
            var totalCasosActual = _context.AlertaSeguimientos
                .Join(_context.UsuarioAsignados,
                    a => a.SeguimientoId,
                    u => u.SeguimientoId,
                    (a, u) => new { a.UltimaFechaSeguimiento, a.EstadoId, u.UsuarioId })
                .Where(x => (string.IsNullOrEmpty(UsuarioID) || x.UsuarioId == UsuarioID)
                            && x.EstadoId != 5
                            && x.UltimaFechaSeguimiento >= FechaInicial
                            && x.UltimaFechaSeguimiento <= FechaFinal)
                .Count();

            // Calcular alertas anteriores
            var totalCasosAnterior = _context.AlertaSeguimientos
                .Join(_context.UsuarioAsignados,
                    a => a.SeguimientoId,
                    u => u.SeguimientoId,
                    (a, u) => new { a.UltimaFechaSeguimiento, a.EstadoId, u.UsuarioId })
                .Where(x => (string.IsNullOrEmpty(UsuarioID) || x.UsuarioId == UsuarioID)
                            && x.EstadoId != 5
                            && x.UltimaFechaSeguimiento >= FechaInicialSemanaAnterior
                            && x.UltimaFechaSeguimiento <= FechaFinalSemanaAnterior)
                .Count();

            // Calcular alertas totales
            var totalCasosGeneral = _context.AlertaSeguimientos
                .Join(_context.UsuarioAsignados,
                    a => a.SeguimientoId,
                    u => u.SeguimientoId,
                    (a, u) => new { a.UltimaFechaSeguimiento, a.EstadoId, u.UsuarioId })
                .Where(x => (string.IsNullOrEmpty(UsuarioID) || x.UsuarioId == UsuarioID) 
                            && x.EstadoId != 5
                            )
                .Count();

            // Retornar un único resultado
            return new GetTotalDashboardResponse
            {
                TotalCasosGeneral = totalCasosGeneral,
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }


        public List<GetDashboardEstadoResponse> RepoDashboardEstados(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {
            var response = (from s1 in _context.Seguimientos
                            join s2 in
                                (from s in _context.Seguimientos
                                 group s by s.NNAId into g
                                 select new { NNAId = g.Key, Id = g.Max(x => x.Id) })
                            on s1.Id equals s2.Id
                            join u in _context.UsuarioAsignados
                            on s1.Id equals u.SeguimientoId
                            where (string.IsNullOrEmpty(UsuarioID) || u.UsuarioId == UsuarioID)
                                && u.FechaAsignacion >= FechaInicial
                                && u.FechaAsignacion <= FechaFinal
                            group s1 by s1.EstadoId into g
                            select new GetDashboardEstadoResponse
                            {
                                EstadoId = g.Key,
                                Cantidad = g.Count()
                            }).ToList();

            return response;

        }

        public List<GetDashboardEstadoResponse> RepoDashboardEstadoNNA(DateTime FechaInicial, DateTime FechaFinal, string UsuarioID)
        {
            var response = (from n in _context.NNAs
                            join s1 in (
                                from s in _context.Seguimientos
                                group s by s.NNAId into g
                                select new { NNAId = g.Key, Id = g.Max(x => x.Id) }
                            ) on n.Id equals s1.NNAId
                            join s2 in _context.Seguimientos on s1.Id equals s2.Id
                            join u in _context.UsuarioAsignados on s2.Id equals u.SeguimientoId
                            where (string.IsNullOrEmpty(UsuarioID) || u.UsuarioId == UsuarioID)
                                && u.FechaAsignacion >= FechaInicial
                                && u.FechaAsignacion <= FechaFinal
                            group new { n, u } by n.estadoId into g
                            select new GetDashboardEstadoResponse
                            {
                                EstadoId = g.Key ?? 0,
                                Cantidad = g.Count()
                            }).ToList();

            return response;

        }

        public List<GetDashboardEstadoResponse> RepoDashboardAlerta(DateTime FechaInicial, DateTime FechaFinal, string UsuarioID)
        {
            // Paso 1: Obtener los máximos Ids por NNAId en memoria
            var maxSeguimientos = _context.Seguimientos
                .GroupBy(s => s.NNAId)
                .Select(g => new { NNAId = g.Key, Id = g.Max(s => s.Id) })
                .ToList();  // Obtener los resultados en memoria

            // Paso 2: Realizar la consulta de AlertaSeguimientos y los joins en memoria
            var alertaSeguimientos = _context.AlertaSeguimientos
                .ToList();  // Ejecutar en memoria para evitar problemas de traducción

            var seguimientos = _context.Seguimientos
                .Where(s => maxSeguimientos.Select(m => m.Id).Contains(s.Id))
                .ToList();  // Filtrar los seguimientos en memoria

            var usuarioAsignados = _context.UsuarioAsignados
                .ToList();  // Obtener todos los usuarios asignados en memoria

            // Realizar la unión de AlertaSeguimientos y Seguimientos en memoria
            var joinedData = alertaSeguimientos.Join(seguimientos,
                a => a.SeguimientoId,
                s => s.Id,
                (a, s) => new { a.EstadoId, s.NNAId, s.Id })
                .Join(usuarioAsignados,
                    s => s.Id,
                    u => u.SeguimientoId,
                    (s, u) => new { s.EstadoId, u.UsuarioId, u.FechaAsignacion })
                .Where(x => (string.IsNullOrEmpty(UsuarioID) || x.UsuarioId == UsuarioID)
                            && x.FechaAsignacion >= FechaInicial
                            && x.FechaAsignacion <= FechaFinal)
                .ToList();  // Ejecutar la consulta en memoria

            // Paso 3: Agrupar los resultados y proyectar el resultado final
            var response = joinedData
                .GroupBy(x => x.EstadoId)
                .Select(g => new GetDashboardEstadoResponse
                {
                    EstadoId = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();  // Ejecutar y devolver el resultado

            return response;
        }




        public List<GetDashboardEstadoResponse> RepoDashboardIntentos(DateTime FechaInicial, DateTime FechaFinal, string UsuarioID)
        {
            
            
            var response = _context.Intentos
                .Join(_context.ContactoNNAs,
                    n => n.ContactoNNAId,               // Clave externa en Intentos
                    co => co.Id,                        // Clave en ContactoNNAs
                    (n, co) => new { n.TipoFallaIntentoId, co.NNAId })  // Proyección
                .Join(_context.NNAs,
                    co => co.NNAId,                     // Clave en ContactoNNAs (desde el join anterior)
                    na => na.Id,                        // Clave en NNAs
                    (co, na) => new { co.TipoFallaIntentoId, na.Id })  // Proyección
                .Join(_context.Seguimientos,
                    na => na.Id,                        // Clave en NNAs (desde el join anterior)
                    s => s.NNAId,                       // Clave en Seguimientos
                    (na, s) => new { na.TipoFallaIntentoId, s.Id })  // Proyección
                .Join(_context.UsuarioAsignados,
                    s => s.Id,                          // Clave en Seguimientos (desde el join anterior)
                    u => u.SeguimientoId,               // Clave en UsuarioAsignados
                    (s, u) => new { s.TipoFallaIntentoId, u.UsuarioId, u.FechaAsignacion })  // Proyección final
                .Where(x => (string.IsNullOrEmpty(UsuarioID) || x.UsuarioId == UsuarioID)
                            && x.FechaAsignacion >= FechaInicial
                            && x.FechaAsignacion <= FechaFinal)
                .GroupBy(x => x.TipoFallaIntentoId)  // Agrupar por TipoResultadoIntentoId
                .Select(g => new GetDashboardEstadoResponse
                {
                    EstadoId = g.Key,                   // Asignar el ID del estado
                    Cantidad = g.Count()                // Contar los registros
                })
                .ToList();

            return response;
        }

        public List<GetDashboardFechaTotalResponse> RepoDashboardAsignadosPorFecha(DateTime fechaInicial, DateTime fechaFinal, string UsuarioID)
        {
            var response = _context.UsuarioAsignados
                .Where(u => (string.IsNullOrEmpty(UsuarioID) || u.UsuarioId == UsuarioID) &&
                            u.FechaAsignacion >= fechaInicial &&
                            u.FechaAsignacion <= fechaFinal)
                .GroupBy(u => u.FechaAsignacion.Date) // Agrupar por fecha
                .Select(g => new GetDashboardFechaTotalResponse
                {
                    FechaAsignacion = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            return response;
        }



        //Dasboard 2

       


        public List<GetEntidadCantidadResponse> RepoDashboardEntidadCantidad(DateTime fechaInicial, DateTime fechaFinal)
        {
            var response = _context.NNAs
            .Where(n => n.FechaNotificacionSIVIGILA == null && n.DateCreated >= fechaInicial &&
                            n.DateCreated <= fechaFinal) // Filtro por FechaNotificacionSIVIGILA
            .Join(
                _context.Entidades, // Segunda tabla
                n => (int) n.EPSId!,       // Clave externa de NNAs
                e => e.Id,          // Clave primaria de Entidades
                (n, e) => new { NombreEntidad = e.Nombre }) // Proyección explícita
            .GroupBy(e => e.NombreEntidad) // Agrupar por el nombre de la entidad
            .Select(g => new GetEntidadCantidadResponse
            {
                Entidad = g.Key!,         // Nombre de la entidad
                Cantidad = g.Count()     // Cantidad de NNAs por entidad
            })
            .ToList();

            return response;
        }


        public List<GetEntidadCantidadResponse> RepoDashboardAgenteCantidad(DateTime FechaInicial, DateTime FechaFinal)
        {
            var response = _context.Seguimientos
                .Join(_context.UsuarioAsignados,
                    s => s.Id,                        // Clave externa en Seguimientos
                    u => u.SeguimientoId,             // Clave en UsuarioAsignados
                    (s, u) => new { u.UsuarioId, u.FechaAsignacion })    // Proyección
                .Join(_context.Users,
                    u => u.UsuarioId,                 // Clave externa en UsuarioAsignados
                    a => a.Id,                        // Clave en AspNetUsers
                    (u, a) => new { a.FullName, u.FechaAsignacion })      // Proyección final
                .Where(x => x.FechaAsignacion >= FechaInicial && x.FechaAsignacion <= FechaFinal)  // Filtrado por rango de fechas
                .GroupBy(x => x.FullName)             // Agrupar por FullName (Nombre completo del agente)
                .Select(g => new GetEntidadCantidadResponse
                {
                    Entidad = g.Key!,                    // Nombre del agente
                    Cantidad = g.Count()               // Cantidad de seguimientos
                })
                .ToList();

            return response;
        }



        public List<GetDashboardCasosCriticosResponse> RepoDashboardCasosCriticos(DateTime fechaInicio, DateTime fechaFin, string? UsuarioID)
        {
            var response = (from n in _context.NNAs
                          join c in _context.CIE10s on n.DiagnosticoId equals c.Id
                          join e in _context.Entidades on (int)n.EPSId! equals e.Id
                          join s in _context.Seguimientos on n.Id equals s.NNAId
                          join als in _context.AlertaSeguimientos on s.Id equals als.SeguimientoId
                          join ua in _context.UsuarioAsignados on s.Id equals ua.SeguimientoId

                            from bm1 in _context.BiStgMunicipio.Where(bm1 => bm1.COD_MUNICIPIO == n.ResidenciaActualMunicipioId).DefaultIfEmpty()
                          from bd1 in _context.BiStgDepartamento.Where(bd1 => bd1.COD_DPTO == bm1.COD_DPTO).DefaultIfEmpty()
                          from bm2 in _context.BiStgMunicipio.Where(bm2 => bm2.COD_MUNICIPIO == n.ResidenciaOrigenMunicipioId).DefaultIfEmpty()
                          from bd2 in _context.BiStgDepartamento.Where(bd2 => bd2.COD_DPTO == bm2.COD_DPTO).DefaultIfEmpty()
                          where (string.IsNullOrEmpty(UsuarioID) || ua.UsuarioId == UsuarioID) &&  s.FechaSeguimiento >= fechaInicio && s.FechaSeguimiento <= fechaFin
                            orderby als.AlertaId, s.FechaSeguimiento
                          select new GetDashboardCasosCriticosResponse
                          {
                              AlertaId = als.AlertaId,
                              PrimerNombre = n.PrimerNombre,
                              SegundoNombre = n.SegundoNombre,
                              PrimerApellido = n.PrimerApellido,
                              SegundoApellido = n.SegundoApellido,
                              FechaNacimiento = n.FechaNacimiento,
                              Diagnostico = c.Nombre,
                              DepartamentoActual = bd1.NOM_DPTO ?? bd2.NOM_DPTO,  // Usamos COALESCE para elegir el primero no nulo
                              MunicipioActual = bm1.Municipio ?? bm2.Municipio,    // Igual para Municipio
                              DepartamentoOrigen = bd2.NOM_DPTO,
                              MunicipioOrigen = bm2.Municipio,
                              FechaNotificacionSIVIGILA = n.FechaNotificacionSIVIGILA,
                              Entidad = e.Nombre,
                              FechaSeguimiento = s.FechaSeguimiento,
                              AgenteSeguimiento = ua.UsuarioId
                          }).Distinct().ToList();


            return response;
        }


        public GetTotalDashboardResponse RepoDashboardTotalSeguimientosCuidador(DateTime FechaInicial, DateTime FechaFinal)
        {
            DateTime startDatePreviousWeek = FechaInicial.AddDays(-7);
            DateTime endDatePreviousWeek = FechaFinal.AddDays(-7);

            var totalCasosActual = _context.Seguimientos
            .Join(
                _context.Seguimientos
                    .GroupBy(s => s.NNAId)
                    .Select(g => new { NNAId = g.Key, Id = g.Max(s => s.Id) }), // Subconsulta para obtener el ID máximo por NNAId
                s1 => s1.Id,  // Clave externa en Seguimientos
                s2 => s2.Id,  // Clave desde la subconsulta
                (s1, s2) => s1 // Proyección final con s1
            )
            .Where(s1 => s1.UltimaActuacionAsunto == "Solicitado por cuidador" && s1.FechaSeguimiento >= FechaInicial &&
                            s1.FechaSeguimiento <= FechaFinal) // Filtro por UltimaActuacionAsunto y fechas
            .Count();

            var totalCasosAnterior = _context.Seguimientos
            .Join(
                _context.Seguimientos
                    .GroupBy(s => s.NNAId)
                    .Select(g => new { NNAId = g.Key, Id = g.Max(s => s.Id) }), // Subconsulta para obtener el ID máximo por NNAId
                s1 => s1.Id,  // Clave externa en Seguimientos
                s2 => s2.Id,  // Clave desde la subconsulta
                (s1, s2) => s1 // Proyección final con s1
            )
            .Where(s1 => s1.UltimaActuacionAsunto == "Solicitado por cuidador" && s1.FechaSeguimiento >= startDatePreviousWeek &&
                            s1.FechaSeguimiento <= endDatePreviousWeek) // Filtro por UltimaActuacionAsunto y fechas
            .Count();

            var totalCasosGeneral = _context.Seguimientos
            .Join(
                _context.Seguimientos
                    .GroupBy(s => s.NNAId)
                    .Select(g => new { NNAId = g.Key, Id = g.Max(s => s.Id) }), // Subconsulta para obtener el ID máximo por NNAId
                s1 => s1.Id,  // Clave externa en Seguimientos
                s2 => s2.Id,  // Clave desde la subconsulta
                (s1, s2) => s1 // Proyección final con s1
            )
            .Where(s1 => s1.UltimaActuacionAsunto == "Solicitado por cuidador") // Filtro por UltimaActuacionAsunto y fechas
            .Count();

            // Retornamos un solo objeto de respuesta
            return new GetTotalDashboardResponse
            {
                TotalCasosGeneral = totalCasosGeneral,
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };

        }


        public GetTotalDashboardResponse RepoDashboardRegistrosPropios(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId)
        {
            DateTime startDatePreviousWeek = FechaInicial.AddDays(-7);
            DateTime endDatePreviousWeek = FechaFinal.AddDays(-7);

            // Obtenemos el conteo de casos actuales y anteriores directamente
            var totalCasosActual = _context.NNAs
                .Where(s => s.DateCreated >= FechaInicial && s.DateCreated <= FechaFinal && s.EAPBId == EntidadId)
                .Count();

            var totalCasosAnterior = _context.NNAs
                .Where(s => s.DateCreated >= startDatePreviousWeek && s.DateCreated <= endDatePreviousWeek && s.EAPBId == EntidadId)
                .Count();

            var totalCasosGeneral = _context.NNAs
                .Where(s => s.EAPBId == EntidadId)
               .Count();

            // Retornamos un solo objeto de respuesta
            return new GetTotalDashboardResponse
            {
                TotalCasosGeneral = totalCasosGeneral,
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }


        public GetTotalDashboardResponse RepoDashboardTotalAlertasEAPB(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId)
        {

            // Calcular alertas actuales
            var totalCasosActual = _context.AlertaSeguimientos
                .Join(_context.UsuarioAsignados,
                    a => a.SeguimientoId,
                    u => u.SeguimientoId,
                    (a, u) => new { a.UltimaFechaSeguimiento, a.EstadoId, u.UsuarioId, a.SeguimientoId })  // Proyectar SeguimientoId
                .Join(_context.Seguimientos,   // Join con Seguimientos usando SeguimientoId
                    a => a.SeguimientoId,
                    s => s.Id,
                    (a, s) => new { a.UltimaFechaSeguimiento, a.EstadoId, a.UsuarioId, s.NNAId })  // Proyectar NNAId
                .Join(_context.NNAs,           // Join con NNAs usando NNAId
                    s => s.NNAId,
                    n => n.Id,
                    (s, n) => new { s.UltimaFechaSeguimiento, s.EstadoId, s.UsuarioId, n.EAPBId })  // Proyectar EAPBId
                .Where(x =>
                            x.EstadoId != 5
                            /*&& x.UltimaFechaSeguimiento >= FechaInicial
                            && x.UltimaFechaSeguimiento <= FechaFinal*/
                            && x.EAPBId == EntidadId)  // Filtro por EAPBId = EntidadId
                .Count();

            // Calcular alertas anteriores
            var totalCasosAnterior = _context.AlertaSeguimientos
               .Join(_context.UsuarioAsignados,
                   a => a.SeguimientoId,
                   u => u.SeguimientoId,
                   (a, u) => new { a.UltimaFechaSeguimiento, a.EstadoId, u.UsuarioId, a.SeguimientoId })  // Proyectar SeguimientoId
               .Join(_context.Seguimientos,   // Join con Seguimientos usando SeguimientoId
                   a => a.SeguimientoId,
                   s => s.Id,
                   (a, s) => new { a.UltimaFechaSeguimiento, a.EstadoId, a.UsuarioId, s.NNAId })  // Proyectar NNAId
               .Join(_context.NNAs,           // Join con NNAs usando NNAId
                   s => s.NNAId,
                   n => n.Id,
                   (s, n) => new { s.UltimaFechaSeguimiento, s.EstadoId, s.UsuarioId, n.EAPBId })  // Proyectar EAPBId
               .Where(x =>
                           x.EstadoId != 5
                           /*&& x.UltimaFechaSeguimiento >= FechaInicialSemanaAnterior
                           && x.UltimaFechaSeguimiento <= FechaFinalSemanaAnterior*/
                           && x.EAPBId == EntidadId)  // Filtro por EAPBId = EntidadId
               .Count();

            // Calcular alertas totales
            var totalCasosGeneral = _context.AlertaSeguimientos
               .Join(_context.UsuarioAsignados,
                   a => a.SeguimientoId,
                   u => u.SeguimientoId,
                   (a, u) => new { a.UltimaFechaSeguimiento, a.EstadoId, u.UsuarioId, a.SeguimientoId })  // Proyectar SeguimientoId
               .Join(_context.Seguimientos,   // Join con Seguimientos usando SeguimientoId
                   a => a.SeguimientoId,
                   s => s.Id,
                   (a, s) => new { a.UltimaFechaSeguimiento, a.EstadoId, a.UsuarioId, s.NNAId })  // Proyectar NNAId
               .Join(_context.NNAs,           // Join con NNAs usando NNAId
                   s => s.NNAId,
                   n => n.Id,
                   (s, n) => new { s.UltimaFechaSeguimiento, s.EstadoId, s.UsuarioId, n.EAPBId })  // Proyectar EAPBId
               .Where(x =>
                           x.EstadoId != 5
                         
                           && x.EAPBId == EntidadId)  // Filtro por EAPBId = EntidadId
               .Count();

            // Retornar un único resultado
            return new GetTotalDashboardResponse
            {
                TotalCasosGeneral = totalCasosGeneral,
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }



        public List<GetDashboardEstadoResponse> RepoDashboardAlertasEAPB(DateTime FechaInicial, DateTime FechaFinal, string EntidadId)
        {
            // Paso 1: Obtener los máximos Ids por NNAId en memoria
            var maxSeguimientos = _context.Seguimientos
                .GroupBy(s => s.NNAId)
                .Select(g => new { NNAId = g.Key, Id = g.Max(s => s.Id) })
                .ToList();  // Obtener los resultados en memoria

            // Paso 2: Realizar la consulta de AlertaSeguimientos y los joins en memoria
            var alertaSeguimientos = _context.AlertaSeguimientos
                .ToList();  // Ejecutar en memoria para evitar problemas de traducción

            var seguimientos = _context.Seguimientos
                .Where(s => maxSeguimientos.Select(m => m.Id).Contains(s.Id))
                .ToList();  // Filtrar los seguimientos en memoria

            var usuarioAsignados = _context.UsuarioAsignados
                .ToList();  // Obtener todos los usuarios asignados en memoria

            // Realizar la unión de AlertaSeguimientos y Seguimientos en memoria
            var joinedData = alertaSeguimientos.Join(seguimientos,
                a => a.SeguimientoId,
                s => s.Id,
                (a, s) => new { a.EstadoId, s.NNAId, s.Id })
                .Join(usuarioAsignados,
                    s => s.Id,
                    u => u.SeguimientoId,
                    (s, u) => new { s.EstadoId, u.UsuarioId, u.FechaAsignacion })
                /*.Where(x => 
                            x.FechaAsignacion >= FechaInicial
                            && x.FechaAsignacion <= FechaFinal)*/
                .ToList();  // Ejecutar la consulta en memoria

            // Paso 3: Agrupar los resultados y proyectar el resultado final
            var response = joinedData
                .GroupBy(x => x.EstadoId)
                .Select(g => new GetDashboardEstadoResponse
                {
                    EstadoId = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();  // Ejecutar y devolver el resultado

            return response;
        }


        public List<GetDashboardEstadoResponse> RepoDashboardAlertasEAPB(DateTime FechaInicial, DateTime FechaFinal, int EAPBId)
        {
            var response = (from n in _context.AlertaSeguimientos
                            join s in _context.Seguimientos on n.Id equals s.NNAId
                            join u in _context.UsuarioAsignados on s.Id equals u.SeguimientoId
                            join nan in _context.NNAs on s.NNAId equals nan.Id
                            where /* u.FechaAsignacion >= FechaInicial && u.FechaAsignacion <= FechaFinal
                                  &&*/ nan.EAPBId == EAPBId
                            group n by n.EstadoId into grouped
                            select new GetDashboardEstadoResponse
                            {
                                EstadoId = grouped.Key,
                                Cantidad = grouped.Count()
                            }).ToList();

            return response;
        }


        public GetDashboardTipoCasosResponse RepoDashboardTipoCasos(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId)
        {
           

            // Obtenemos el conteo de casos actuales y anteriores directamente
            var ConAlerta =
                (from n in _context.NNAs
                 join s in _context.Seguimientos on n.Id equals s.NNAId
                 join u in _context.UsuarioAsignados on s.Id equals u.SeguimientoId
                 join nan in
                     (from s1 in _context.AlertaSeguimientos
                      join s2 in
                         (from s in _context.AlertaSeguimientos
                          group s by s.SeguimientoId into g
                          select new
                          {
                              SeguimientoId = g.Key,
                              MaxId = g.Max(x => x.Id)
                          })
                      on s1.Id equals s2.MaxId
                      select s1)
                 on s.Id equals nan.SeguimientoId
                 where /*u.FechaAsignacion >= FechaInicial
                       && u.FechaAsignacion <= FechaFinal
                       &&*/ n.EAPBId == EntidadId
                       && !new[] { 5 }.Contains(nan.EstadoId!)
                 select n).Count();


        

            var SinAlerta = (from n in _context.NNAs
                             join s in _context.Seguimientos on n.Id equals s.NNAId
                             join u in _context.UsuarioAsignados on s.Id equals u.SeguimientoId
                             join nan in
                                 (from s1 in _context.AlertaSeguimientos
                                  join s2 in
                                     (from s in _context.AlertaSeguimientos
                                      group s by s.SeguimientoId into g
                                      select new
                                      {
                                          SeguimientoId = g.Key,
                                          MaxId = g.Max(x => x.Id)
                                      })
                                  on s1.Id equals s2.MaxId into nanJoin
                                  from nan in nanJoin.DefaultIfEmpty() // Left Join
                                  select s1)
                             on s.Id equals nan.SeguimientoId into leftJoinAlerta
                             from nan in leftJoinAlerta.DefaultIfEmpty() // Left Join
                             where /*u.FechaAsignacion >= FechaInicial
                                   && u.FechaAsignacion <= FechaFinal
                                   &&*/ n.EAPBId == EntidadId
                                   && nan.AlertaId == null
                             select n).Count();



            // Retornamos un solo objeto de respuesta
            return new GetDashboardTipoCasosResponse
            {
                ConAlerta = ConAlerta,
                SinAlerta = SinAlerta,
               
            };
        }


        public List<GetDashboardCasosCriticosEapbResponse> RepoDashboardCasosCriticosEAPB( int EntidadId)
        {
            var resultado = (from n in _context.NNAs
                             join c in _context.CIE10s on n.DiagnosticoId equals c.Id
                             join s in _context.Seguimientos on n.Id equals s.NNAId
                             join als in _context.AlertaSeguimientos on s.Id equals als.SeguimientoId
                             where /*s.FechaSeguimiento >= FechaInicio
                                   && s.FechaSeguimiento <= FechaFin
                                   &&*/ n.EAPBId == EntidadId
                             orderby als.AlertaId, s.FechaSeguimiento
                             select new GetDashboardCasosCriticosEapbResponse
                             {
                                 AlertaId = als.AlertaId,
                                 PrimerNombre = n.PrimerNombre,
                                 SegundoNombre = n.SegundoNombre,
                                 PrimerApellido = n.PrimerApellido,
                                 SegundoApellido = n.SegundoApellido,
                                 FechaNacimiento = n.FechaNacimiento,
                                 Diagnostico = c.Nombre,
                                 FechaSeguimiento = als.DateCreated,
                                
                             }).Distinct().ToList();


            return resultado;

        }


    }



}