using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;
using Core.Modelos;
using MSAuthentication.Core.DTOs;
using Core.Response;


namespace Infra.Repositorios
{
    public class DashboardRepo : IDashboardRepo
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepo(ApplicationDbContext context) => _context = context;

        public GetTotalDashboardResponse RepoDashboardTotalCasos(DateTime FechaInicial, DateTime FechaFinal, string? EntidadId)
        {
            DateTime startDatePreviousWeek = FechaInicial.AddDays(-7);
            DateTime endDatePreviousWeek = FechaFinal.AddDays(-7);

            // Obtenemos el conteo de casos actuales y anteriores directamente
            var totalCasosActual = _context.Seguimientos
                .Where(s => s.FechaSeguimiento >= FechaInicial && s.FechaSeguimiento <= FechaFinal)
                .Count();

            var totalCasosAnterior = _context.Seguimientos
                .Where(s => s.FechaSeguimiento >= startDatePreviousWeek && s.FechaSeguimiento <= endDatePreviousWeek)
                .Count();

            // Retornamos un solo objeto de respuesta
            return new GetTotalDashboardResponse
            {
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }



        public GetTotalDashboardResponse RepoDashboardTotalRegistros(DateTime FechaInicial, DateTime FechaFinal, string? EntidadId)
        {
           
            DateTime startDatePreviousWeek = FechaInicial.AddDays(-7);
            DateTime endDatePreviousWeek = FechaFinal.AddDays(-7);

            // Obtenemos el conteo de casos actuales directamente
            var totalCasosActual = _context.Seguimientos
                .Where(s => s.DateCreated >= FechaInicial && s.FechaSeguimiento <= FechaFinal)
                .Count();

            // Obtenemos el conteo de casos de la semana anterior
            var totalCasosAnterior = _context.Seguimientos
                .Where(s => s.DateCreated >= startDatePreviousWeek && s.FechaSeguimiento <= endDatePreviousWeek)
                .Count();

            // Retornamos un único resultado
            return new GetTotalDashboardResponse
            {
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }


        public GetTotalDashboardResponse RepoDashboardMisCasos(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {
            
            DateTime FechaInicialSemanaAnterior = FechaInicial.AddDays(-7);
            DateTime FechaFinalSemanaAnterior = FechaFinal.AddDays(-7);


            // Obtenemos el conteo de casos actuales directamente
            var totalCasosActual = _context.UsuarioAsignados
                .Where(ua => ua.UsuarioId == UsuarioID &&
                             ua.FechaAsignacion >= FechaInicial &&
                             ua.FechaAsignacion <= FechaFinal)
                .Count();

            // Obtenemos el conteo de casos de la semana anterior
            var totalCasosAnterior = _context.UsuarioAsignados
                .Where(ua => ua.UsuarioId == UsuarioID &&
                             ua.FechaAsignacion >= FechaInicialSemanaAnterior &&
                             ua.FechaAsignacion <= FechaFinalSemanaAnterior)
                .Count();

            // Retornamos un único resultado
            return new GetTotalDashboardResponse
            {
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
                .Where(x => x.UsuarioId == UsuarioID
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
                .Where(x => x.UsuarioId == UsuarioID
                            && x.EstadoId != 5
                            && x.UltimaFechaSeguimiento >= FechaInicialSemanaAnterior
                            && x.UltimaFechaSeguimiento <= FechaFinalSemanaAnterior)
                .Count();

            // Retornar un único resultado
            return new GetTotalDashboardResponse
            {
                TotalCasosActual = totalCasosActual,
                TotalCasosAnterior = totalCasosAnterior
            };
        }


        public List<GetDashboardEstadoResponse> RepoDashboardEstados(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {
            var response = _context.Seguimientos
                .Join(_context.UsuarioAsignados,
                    s => s.Id,                  // Clave externa en Seguimientos
                    u => u.SeguimientoId,        // Clave en UsuarioAsignados
                    (s, u) => new { s.EstadoId, u.UsuarioId, u.FechaAsignacion })  // Proyección
                .Where(x => (string.IsNullOrEmpty(UsuarioID) || x.UsuarioId == UsuarioID)
                            && x.FechaAsignacion >= FechaInicial
                            && x.FechaAsignacion <= FechaFinal)
                .GroupBy(x => x.EstadoId)        // Agrupar por EstadoId
                .Select(g => new GetDashboardEstadoResponse
                {
                    EstadoId = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            return response;
        }

        public List<GetDashboardEstadoResponse> RepoDashboardEstadoNNA(DateTime FechaInicial, DateTime FechaFinal, string UsuarioID)
        {
            var response = _context.NNAs
                .Join(_context.Seguimientos,
                    n => n.Id,                  // Clave externa en NNAs
                    s => s.NNAId,               // Clave en Seguimientos
                    (n, s) => new { n.estadoId, s.Id, s.NNAId })  // Proyección
                .Join(_context.UsuarioAsignados,
                    s => s.Id,                  // Clave en Seguimientos (desde el join anterior)
                    u => u.SeguimientoId,       // Clave en UsuarioAsignados
                    (s, u) => new { s.estadoId, u.UsuarioId, u.FechaAsignacion })  // Proyección final
                .Where(x => x.UsuarioId == UsuarioID
                            && x.FechaAsignacion >= FechaInicial
                            && x.FechaAsignacion <= FechaFinal)
                .GroupBy(x => x.estadoId)        // Agrupar por EstadoId
                .Select(g => new GetDashboardEstadoResponse
                {
                    EstadoId = g.Key ?? 0,
                    Cantidad = g.Count()
                })
                .ToList();

            return response;
        }


        public List<GetDashboardEstadoResponse> RepoDashboardAlerta(DateTime FechaInicial, DateTime FechaFinal, string UsuarioID)
        {
            var response = _context.AlertaSeguimientos
                .Join(_context.Seguimientos,
                    n => n.Id,                  // Clave externa en NNAs
                    s => s.NNAId,               // Clave en Seguimientos
                    (n, s) => new { n.EstadoId, s.Id, s.NNAId })  // Proyección
                .Join(_context.UsuarioAsignados,
                    s => s.Id,                  // Clave en Seguimientos (desde el join anterior)
                    u => u.SeguimientoId,       // Clave en UsuarioAsignados
                    (s, u) => new { s.EstadoId, u.UsuarioId, u.FechaAsignacion })  // Proyección final
                .Where(x => (string.IsNullOrEmpty(UsuarioID) || x.UsuarioId == UsuarioID)
                            && x.FechaAsignacion >= FechaInicial
                            && x.FechaAsignacion <= FechaFinal)
                .GroupBy(x => x.EstadoId)        // Agrupar por EstadoId
                .Select(g => new GetDashboardEstadoResponse
                {
                    EstadoId = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

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
                .Where(x => x.UsuarioId == UsuarioID
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

        public List<GetDashboardFechaTotalResponse> RepoDashboardAsignadosPorFecha(DateTime fechaInicial, DateTime fechaFinal, string UsuarioId)
        {
            var response = _context.UsuarioAsignados
                .Where(u => u.UsuarioId == UsuarioId &&
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


        public List<GetEntidadCantidadResponse> RepoDashboardEntidadCantidad(DateTime fechaInicial, DateTime fechaFinal)
        {
            var response = _context.NNAs
            .Where(n => n.FechaNotificacionSIVIGILA == null && n.DateCreated >= fechaInicial &&
                            n.DateCreated <= fechaFinal) // Filtro por FechaNotificacionSIVIGILA
            .Join(
                _context.Entidades, // Segunda tabla
                n => (int) n.EPSId,       // Clave externa de NNAs
                e => e.Id,          // Clave primaria de Entidades
                (n, e) => new { NombreEntidad = e.Nombre }) // Proyección explícita
            .GroupBy(e => e.NombreEntidad) // Agrupar por el nombre de la entidad
            .Select(g => new GetEntidadCantidadResponse
            {
                Entidad = g.Key,         // Nombre de la entidad
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
                .Join(_context.AspNetUsers,
                    u => u.UsuarioId,                 // Clave externa en UsuarioAsignados
                    a => a.Id,                        // Clave en AspNetUsers
                    (u, a) => new { a.FullName, u.FechaAsignacion })      // Proyección final
                .Where(x => x.FechaAsignacion >= FechaInicial && x.FechaAsignacion <= FechaFinal)  // Filtrado por rango de fechas
                .GroupBy(x => x.FullName)             // Agrupar por FullName (Nombre completo del agente)
                .Select(g => new GetEntidadCantidadResponse
                {
                    Entidad = g.Key,                    // Nombre del agente
                    Cantidad = g.Count()               // Cantidad de seguimientos
                })
                .ToList();

            return response;
        }



        public List<GetDashboardCasosCriticosResponse> RepoDashboardCasosCriticos(DateTime fechaInicio, DateTime fechaFin)
        {
            var response = (from n in _context.NNAs
                          join c in _context.CIE10s on n.DiagnosticoId equals c.Id
                          join e in _context.Entidades on (int)n.EPSId equals e.Id
                          join s in _context.Seguimientos on n.Id equals s.NNAId
                          join als in _context.AlertaSeguimientos on s.Id equals als.SeguimientoId
                          join ua in _context.UsuarioAsignados on s.Id equals ua.SeguimientoId

                            from bm1 in _context.BiStgMunicipio.Where(bm1 => bm1.COD_MUNICIPIO == n.ResidenciaActualMunicipioId).DefaultIfEmpty()
                          from bd1 in _context.BiStgDepartamento.Where(bd1 => bd1.COD_DPTO == bm1.COD_DPTO).DefaultIfEmpty()
                          from bm2 in _context.BiStgMunicipio.Where(bm2 => bm2.COD_MUNICIPIO == n.ResidenciaOrigenMunicipioId).DefaultIfEmpty()
                          from bd2 in _context.BiStgDepartamento.Where(bd2 => bd2.COD_DPTO == bm2.COD_DPTO).DefaultIfEmpty()
                          where s.FechaSeguimiento >= fechaInicio && s.FechaSeguimiento <= fechaFin
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





    }
}