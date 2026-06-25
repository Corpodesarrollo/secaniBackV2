using Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace Infra.Services.Email
{
    /// <summary>
    /// Lee credenciales SMTP desde tabla EmailConfigurations. Comportamiento actual
    /// EC2 + Gmail. NO mover lógica aquí: solo facade sobre _context.EmailConfigurations
    /// para que NotificacionRepo dependa del provider, no de la tabla directamente.
    /// </summary>
    public class DbEmailCredentialsProvider : IEmailCredentialsProvider
    {
        private readonly ApplicationDbContext _context;

        public DbEmailCredentialsProvider(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmailCredentials?> GetAsync(CancellationToken ct = default)
        {
            var cfg = await _context.EmailConfigurations
                .Where(c => !c.IsDeleted)
                .FirstOrDefaultAsync(ct);
            if (cfg == null) return null;
            return new EmailCredentials
            {
                SmtpServer = cfg.SmtpServer,
                Port = cfg.Port > 0 ? cfg.Port : 587,
                EnableSsl = cfg.EnableSsl,
                UserName = cfg.UserName,
                Password = cfg.Password,
                FromEmail = !string.IsNullOrWhiteSpace(cfg.FromEmail) ? cfg.FromEmail : cfg.UserName
            };
        }
    }
}
