﻿using Core.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.response;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;


namespace Core.Interfaces.Repositorios
{
    public interface ISeguimientoRepo
    {
        public Seguimiento GetById(long id);
        public List<GetSeguimientoResponse> RepoSeguimientoUsuario(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal);
        public int RepoSeguimientoActualizacionFecha(PutSeguimientoActualizacionFechaRequest request);
        public int RepoSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request);
        public List<GetSeguimientoFestivoResponse> RepoSeguimientoFestivo(DateTime FechaInicial, DateTime FechaFinal);
        public List<GetSeguimientoHorarioAgenteResponse> RepoSeguimientoHorarioAgente(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal);
        public List<GetSeguimientoAgentesResponse> RepoSeguimientoAgentes(string UsuarioId);
        public void SetEstadoDiagnosticoTratamiento(EstadoDiagnosticoTratamientoRequest request);
        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request);
        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request);
        public void SetDificultadesProceso(DificultadesProcesoRequest request);
        public void SetAdherenciaProceso (AdherenciaProcesoRequest request);
        
    }
}
