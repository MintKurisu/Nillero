using Nillero.Core.Domain.Entities.Social;
using Nillero.Core.Domain.Interfaces.Social;
using Nillero.Infrastructure.Persistence.Contexts;
using Nillero.Infrastructure.Persistence.Repositories.Base;

namespace Nillero.Infrastructure.Persistence.Repositories.Social
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        public PostRepository(NilleroAppContext context) : base(context)
        {

        }
    }
}
