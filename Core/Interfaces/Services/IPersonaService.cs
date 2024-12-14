using Core.DTOs;
using Core.DTOs.Reportes;
using Core.Modelos;

namespace Core.Interfaces.Services
{
    public interface IPersonaService
    {
        /// <summary>
        /// Obtiene la identificación vigente de la persona.
        /// </summary>
        /// <param name="tipoIdentificacion">Tipo de identificación (RC, TI, CC, etc.)</param>
        /// <param name="nroIdentificacion">Número de identificación</param>
        /// <param name="fechaExpedicion">Fecha de expedición (opcional)</param>
        /// <returns>Identificación vigente</returns>
        Task<object> GetIdVigenteAsync(string tipoIdentificacion, string nroIdentificacion, string fechaExpedicion = null);

        /// <summary>
        /// Obtiene todas las identificaciones asociadas a una persona.
        /// </summary>
        /// <param name="tipoIdentificacion">Tipo de identificación (RC, TI, CC, etc.)</param>
        /// <param name="nroIdentificacion">Número de identificación</param>
        /// <param name="fechaExpedicion">Fecha de expedición (opcional)</param>
        /// <returns>Lista de identificaciones asociadas</returns>
        Task<object> GetIdAllAsync(string tipoIdentificacion, string nroIdentificacion, string fechaExpedicion = null);

        /// <summary>
        /// Obtiene el tipo de identificación vigente para la persona.
        /// </summary>
        /// <param name="nroIdentificacion">Número de identificación</param>
        /// <returns>Tipo de identificación vigente</returns>
        Task<object> GetTipoIdentificacionVigenteAsync(string nroIdentificacion);
    }
}
