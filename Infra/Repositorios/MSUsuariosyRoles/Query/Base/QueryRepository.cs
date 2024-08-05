using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Query.Base;
using Microsoft.Extensions.Configuration;

namespace Infra.Repositorios.MSUsuariosyRoles.Query.Base
{
    public class QueryRepository<T> : DbConnector, IQueryRepository<T> where T : class
    {
        public QueryRepository(IConfiguration configuration)
            : base(configuration)
        {

        }
    }
}
