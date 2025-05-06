using Core.DTOs;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios
{
    public class EnviarRespuestaRepo(ApplicationDbContext db) : IEnviarRespuesta
    {
        private readonly ApplicationDbContext db = db;

        public async Task<bool> EnviarRespuesta(EnviarRespuestaDto data)
        {
            var emailConfig = await db.EmailConfigurations.FirstOrDefaultAsync();
            if (emailConfig == null)
                return false;

            emailConfig.SendEmail([data.Para], data.Cc, null, data.Asunto, $"{data.Mensaje}\n\n{data.Firma}", data.Archivo != null ? [data.Archivo] : null);
            return true;
        }
    }
}
