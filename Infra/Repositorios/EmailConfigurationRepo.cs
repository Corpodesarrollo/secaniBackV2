using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios.Common;
using Core.Modelos;
using Microsoft.EntityFrameworkCore;


namespace Infra.Repositorios
{
    public class EmailConfigurationRepo : IEmailConfigurationRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<EmailConfiguration> _repository;

        public EmailConfigurationRepo(
            ApplicationDbContext context,
            IGenericRepository<EmailConfiguration> repository
            )
        {
            _context = context;
            _repository = repository;
        }


        public IQueryable<EmailConfigurationDto> SelectBase()
        {
            try
            {
                return from e in _context.EmailConfigurations
                       select new EmailConfigurationDto
                       {
                           SmtpServer = e.SmtpServer,
                           Port = e.Port,
                           EnableSsl = e.EnableSsl,
                           UserName = e.UserName,
                           Password = e.Password
                       };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<EmailConfigurationDto>().AsQueryable();
            }
        }

        public async Task<EmailConfigurationDto?> Get()
        {
            try
            {
                var nna = await SelectBase().FirstOrDefaultAsync();
                return nna;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}