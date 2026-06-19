using Core.Interfaces.Repositorios;
using Core.response;


namespace Infra.Repositorios
{
    public class DashboardRepo : IDashboardRepo
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepo(ApplicationDbContext context) => _context = context;

        public GetTotalDashboardResponse RepoDashboardTotalCasos(DateTime FechaInicial, DateTime FechaFinal)
        {
            // HU SECANI-RQ07-HU01: KPI global, ignora FechaInicial/FechaFinal.
            // Porcentaje "aumento/disminucion esta semana" => comparar NNA creados en los
            // ultimos 7 dias contra los 7 dias previos (semana anterior).
            var hoy = DateTime.Now.Date;
            var inicioSemana = hoy.AddDays(-7);
            var inicioSemanaAnterior = hoy.AddDays(-14);

            var totalCasosActual = _context.NNAs
                .Where(s => s.FechaIngresoEstrategia >= inicioSemana
                            && s.FechaIngresoEstrategia < hoy.AddDays(1))
                .Count();

            var totalCasosAnterior = _context.NNAs
                .Where(s => s.FechaIngresoEstrategia >= inicioSemanaAnterior
                            && s.FechaIngresoEstrategia < inicioSemana)
                .Count();

            var totalCasosGeneral = _context.NNAs.Count();

            return new GetTotalDashboardResponse
            {
                TotalCasosGeneral = totalCasosGeneral,
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }

        public GetTotalDashboardResponse RepoDashboardTotalRegistros(DateTime FechaInicial, DateTime FechaFinal, string? EntidadId)
        {
            if (FechaInicial == DateTime.MinValue) FechaInicial = DateTime.Now.AddMonths(-1);
            if (FechaFinal == DateTime.MinValue) FechaFinal = DateTime.Now;

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
            // HU SECANI-RQ02-HU01: "Mis Casos" = NNAs distintos asignados al agente. Antes
            // contaba filas de UsuarioAsignados (con duplicados cuando el mismo seguimiento
            // se asignaba varias veces). El listado /gestion/seguimientos cuenta NNAs
            // distintos + excluye estadoId == 10 (NNA fallecido / inactivo) -> KPI 21 vs
            // lista 14 confuso. Se alinean ambos. "% esta semana" => NNAs asignados en los
            // ultimos 7 dias vs los 7 dias previos.
            var hoy = DateTime.Now.Date;
            var inicioSemana = hoy.AddDays(-7);
            var inicioSemanaAnterior = hoy.AddDays(-14);

            var baseQuery = from ua in _context.UsuarioAsignados
                            join s in _context.Seguimientos on ua.SeguimientoId equals s.Id
                            join n in _context.NNAs on s.NNAId equals n.Id
                            where (string.IsNullOrEmpty(UsuarioID) || ua.UsuarioId == UsuarioID)
                                  && n.estadoId != 10
                            select new { NNAId = n.Id, ua.FechaAsignacion };

            var totalCasosGeneral = baseQuery.Select(x => x.NNAId).Distinct().Count();

            var totalCasosActual = baseQuery
                .Where(x => x.FechaAsignacion >= inicioSemana && x.FechaAsignacion < hoy.AddDays(1))
                .Select(x => x.NNAId).Distinct().Count();

            var totalCasosAnterior = baseQuery
                .Where(x => x.FechaAsignacion >= inicioSemanaAnterior && x.FechaAsignacion < inicioSemana)
                .Select(x => x.NNAId).Distinct().Count();


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
            if (FechaInicial == DateTime.MinValue) FechaInicial = DateTime.Now.AddMonths(-1);
            if (FechaFinal == DateTime.MinValue) FechaFinal = DateTime.Now;

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
                .DefaultIfEmpty()
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
                .Where(ua => ua.UsuarioId == UsuarioID &&
                             ua.FechaAsignacion >= fechaInicial &&
                             ua.FechaAsignacion <= fechaFinal)
                .Join(
                    _context.Seguimientos
                        .Join(
                            _context.Seguimientos
                                .GroupBy(s => s.NNAId)
                                .Select(g => new { NNAId = g.Key, Id = g.Max(s => s.Id) }),
                            s1 => s1.Id,
                            s2 => s2.Id,
                            (s1, s2) => s1
                        ),
                    ua => ua.SeguimientoId,
                    s1 => s1.Id,
                    (ua, s1) => ua
                )
                .GroupBy(ua => ua.FechaAsignacion)
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
                n => (int)n.EPSId!,       // Clave externa de NNAs
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
                            join alt in _context.Alertas on als.AlertaId equals alt.Id
                            join s1 in
                                          (from s1 in _context.Seguimientos
                                           join s2 in
                                               (from s in _context.Seguimientos
                                                group s by s.NNAId into g
                                                select new { NNAId = g.Key, Id = g.Max(x => x.Id) })
                                           on s1.Id equals s2.Id
                                           select s1)
                                      on ua.SeguimientoId equals s1.Id

                            from bm1 in _context.BiStgMunicipio.Where(bm1 => bm1.COD_MUNICIPIO == n.ResidenciaActualMunicipioId).DefaultIfEmpty()
                            from bd1 in _context.BiStgDepartamento.Where(bd1 => bd1.COD_DPTO == bm1.COD_DPTO).DefaultIfEmpty()
                            from bm2 in _context.BiStgMunicipio.Where(bm2 => bm2.COD_MUNICIPIO == n.ResidenciaOrigenMunicipioId).DefaultIfEmpty()
                            from bd2 in _context.BiStgDepartamento.Where(bd2 => bd2.COD_DPTO == bm2.COD_DPTO).DefaultIfEmpty()
                            where (string.IsNullOrEmpty(UsuarioID) || ua.UsuarioId == UsuarioID) && s.FechaSeguimiento >= fechaInicio && s.FechaSeguimiento <= fechaFin
                            orderby als.AlertaId, s.FechaSeguimiento
                            select new GetDashboardCasosCriticosResponse
                            {
                                AlertaId = als.AlertaId,
                                SubcategoriaId = alt.SubcategoriaId,
                                Alias = alt.Alias,
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
            if (FechaInicial == DateTime.MinValue) FechaInicial = DateTime.Now.AddMonths(-1);
            if (FechaFinal == DateTime.MinValue) FechaFinal = DateTime.Now;

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
            // HU SECANI-RQ07-HU01: KPI global, ignora FechaInicial/FechaFinal. Filtra por
            // EAPBId. "% este mes" => comparar NNA creados ultimos 30 dias contra los 30
            // dias previos (mes anterior).
            var hoy = DateTime.Now.Date;
            var inicioMes = hoy.AddDays(-30);
            var inicioMesAnterior = hoy.AddDays(-60);

            var totalCasosGeneral = _context.NNAs
                .Where(s => !EntidadId.HasValue || s.EAPBId == EntidadId)
                .Count();

            var totalCasosActual = _context.NNAs
                .Where(s => s.DateCreated >= inicioMes && s.DateCreated < hoy.AddDays(1)
                            && (!EntidadId.HasValue || s.EAPBId == EntidadId))
                .Count();

            var totalCasosAnterior = _context.NNAs
                .Where(s => s.DateCreated >= inicioMesAnterior && s.DateCreated < inicioMes
                            && (!EntidadId.HasValue || s.EAPBId == EntidadId))
                .Count();

            return new GetTotalDashboardResponse
            {
                TotalCasosGeneral = totalCasosGeneral,
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }


        public GetTotalDashboardResponse RepoDashboardTotalAlertasEAPB(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId)
        {
            // HU SECANI-RQ07-HU01: KPI global, ignora FechaInicial/FechaFinal. Filtra por
            // EAPBId del NNA. Solo cuenta las alertas abiertas (EstadoId != 5) del ULTIMO
            // seguimiento por NNA (misma logica que /gestionar-alertas y el tablero
            // "Alertas Pendientes" para evitar inconsistencias visuales: KPI 29 vs lista 3).
            // "% esta semana" => alertas con UltimaFechaSeguimiento ultimos 7 dias vs 7 dias
            // previos.
            var hoy = DateTime.Now.Date;
            var inicioSemana = hoy.AddDays(-7);
            var inicioSemanaAnterior = hoy.AddDays(-14);

            var ultimoSegPorNNA = _context.Seguimientos
                .GroupBy(s => s.NNAId)
                .Select(g => g.Max(x => x.Id));

            // HU SECANI-RQ07-HU03 (extension): incluir tambien alertas con notificacion
            // enviada a la EAPB del usuario aunque la EAPB del NNA sea otra.
            var alertasNotifAEntidad = !EntidadId.HasValue
                ? null
                : _context.NotificacionesEntidad
                    .Where(ne => ne.EntidadId == EntidadId && !ne.IsDeleted)
                    .Select(ne => ne.AlertaSeguimientoId ?? 0);

            var baseQuery = from a in _context.AlertaSeguimientos
                            join s in _context.Seguimientos on a.SeguimientoId equals s.Id
                            join n in _context.NNAs on s.NNAId equals n.Id
                            where a.EstadoId != 5
                                  && ultimoSegPorNNA.Contains(a.SeguimientoId)
                                  && (!EntidadId.HasValue
                                      || n.EAPBId == EntidadId
                                      || alertasNotifAEntidad!.Contains(a.Id))
                            select new { a.UltimaFechaSeguimiento };

            var totalCasosGeneral = baseQuery.Count();
            var totalCasosActual = baseQuery.Count(x => x.UltimaFechaSeguimiento >= inicioSemana
                                                        && x.UltimaFechaSeguimiento < hoy.AddDays(1));
            var totalCasosAnterior = baseQuery.Count(x => x.UltimaFechaSeguimiento >= inicioSemanaAnterior
                                                          && x.UltimaFechaSeguimiento < inicioSemana);

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
                           .Where(x =>
                                       x.FechaAsignacion >= FechaInicial
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


        public List<GetDashboardEstadoResponse> RepoDashboardAlertasEAPB(DateTime FechaInicial, DateTime FechaFinal, int EAPBId)
        {
            // BUG-LZ 2026-06-18: JOIN previo "n.Id equals s.NNAId" mezclaba AlertaSeguimiento.Id
            // con Seguimiento.NNAId (FKs distintas), produciendo cardinalidad/estados erroneos
            // en el pie chart "Alertas". El JOIN correcto es AlertaSeguimiento.SeguimientoId
            // == Seguimiento.Id. Tambien se activa el filtro por EAPBId del NNA.
            // HU SECANI-RQ07-HU01: el pie "Alertas" agrupa por CATEGORIA de la alerta
            // (no por EstadoId). Se navega AlertaSeguimiento -> Alerta -> SubCategoriaAlerta
            // -> CategoriaAlerta para llegar a la categoria final.
            var response = (from a in _context.AlertaSeguimientos
                            join s in _context.Seguimientos on a.SeguimientoId equals s.Id
                            join nan in _context.NNAs on s.NNAId equals nan.Id
                            join al in _context.Alertas on a.AlertaId equals al.Id
                            join sca in _context.TPSubCategoriaAlerta on al.SubcategoriaId equals sca.Id
                            join ca in _context.TPCategoriaAlerta on sca.CategoriaAlertaId equals ca.Id
                            where a.UltimaFechaSeguimiento >= FechaInicial
                                  && a.UltimaFechaSeguimiento <= FechaFinal
                                  && (EAPBId == 0 || nan.EAPBId == EAPBId)
                            group a by ca.Id into grouped
                            select new GetDashboardEstadoResponse
                            {
                                EstadoId = grouped.Key,
                                Cantidad = grouped.Count()
                            }).ToList();

            return response;
        }


        public GetDashboardTipoCasosResponse RepoDashboardTipoCasos(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId)
        {
            // HU SECANI-RQ07-HU01: pie "Alertas abiertas EAPB". Distribucion Abiertas vs
            // Cerradas (EstadoId != 5 vs == 5) sobre AlertaSeguimientos del EAPB en el
            // rango de fechas seleccionado (UltimaFechaSeguimiento).
            var alertasQ = from a in _context.AlertaSeguimientos
                           join s in _context.Seguimientos on a.SeguimientoId equals s.Id
                           join n in _context.NNAs on s.NNAId equals n.Id
                           where a.UltimaFechaSeguimiento >= FechaInicial
                                 && a.UltimaFechaSeguimiento <= FechaFinal
                                 && (!EntidadId.HasValue || n.EAPBId == EntidadId)
                           select a.EstadoId;

            var abiertas = alertasQ.Count(e => e != 5);
            var cerradas = alertasQ.Count(e => e == 5);

            return new GetDashboardTipoCasosResponse
            {
                ConAlerta = abiertas,
                SinAlerta = cerradas
            };
        }

        public GetDashboardTipoCasosResponse RepoDashboardTipoCasos_LEGACY(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId)
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
                 where u.FechaAsignacion >= FechaInicial
                       && u.FechaAsignacion <= FechaFinal
                       && (!EntidadId.HasValue || n.EAPBId == EntidadId)
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
                             where u.FechaAsignacion >= FechaInicial
                                   && u.FechaAsignacion <= FechaFinal
                                   && (!EntidadId.HasValue || n.EAPBId == EntidadId)
                                   && nan == null
                             select n).Count();



            // Retornamos un solo objeto de respuesta
            return new GetDashboardTipoCasosResponse
            {
                ConAlerta = ConAlerta,
                SinAlerta = SinAlerta,

            };
        }


        public List<GetDashboardCasosCriticosEapbResponse> RepoDashboardCasosCriticosEAPB(string EntidadId, DateTime FechaInicial, DateTime FechaFinal)
        {
            // HU SECANI-RQ07-HU01: el tablero "Alertas Pendientes" muestra una fila por
            // alerta abierta del ULTIMO seguimiento por NNA (mismo criterio que el listado
            // oficial /gestionar-alertas, evita duplicados cuando un NNA tiene historico de
            // seguimientos). El filtro de fechas del dashboard solo aplica a graficos, no
            // a este tablero.
            int? eapbFiltro = int.TryParse(EntidadId, out var parsed) && parsed > 0 ? parsed : null;

            var ultimoSegPorNNA = _context.Seguimientos
                .GroupBy(s => s.NNAId)
                .Select(g => g.Max(x => x.Id));

            // HU SECANI-RQ07-HU03 (extension): incluir alertas con notificacion enviada
            // a la EAPB del usuario aunque la EAPB del NNA sea otra.
            var alertasNotifAEntidad = eapbFiltro == null
                ? null
                : _context.NotificacionesEntidad
                    .Where(ne => ne.EntidadId == eapbFiltro && !ne.IsDeleted)
                    .Select(ne => ne.AlertaSeguimientoId ?? 0);

            var resultado = (from als in _context.AlertaSeguimientos
                             where als.EstadoId != 5
                                   && ultimoSegPorNNA.Contains(als.SeguimientoId)
                             join s in _context.Seguimientos on als.SeguimientoId equals s.Id
                             join n in _context.NNAs on s.NNAId equals n.Id
                             join c0 in _context.CIE10s on n.DiagnosticoId equals c0.Id into cJoin
                             from c in cJoin.DefaultIfEmpty()
                             where eapbFiltro == null
                                   || n.EAPBId == eapbFiltro
                                   || alertasNotifAEntidad!.Contains(als.Id)
                             orderby als.DateCreated descending
                             select new GetDashboardCasosCriticosEapbResponse
                             {
                                 AlertaId = als.Id,
                                 PrimerNombre = n.PrimerNombre,
                                 SegundoNombre = n.SegundoNombre,
                                 PrimerApellido = n.PrimerApellido,
                                 SegundoApellido = n.SegundoApellido,
                                 FechaNacimiento = n.FechaNacimiento,
                                 Diagnostico = c != null ? c.Nombre : "",
                                 FechaSeguimiento = als.DateCreated,
                                 NNaId = n.Id
                             }).ToList();

            return resultado;
        }

        // BUG-LZ-087: lookup TPEAPB.Id por NIT (long?) -> int? (Id PK). Devuelve null si no existe.
        public int? GetEAPBIdByNit(long nit)
        {
            return _context.TPEAPB
                .Where(e => e.NIT == nit)
                .Select(e => (int?)e.Id)
                .FirstOrDefault();
        }

    }



}