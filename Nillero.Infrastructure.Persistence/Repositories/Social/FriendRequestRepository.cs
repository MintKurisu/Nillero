using Nillero.Core.Domain.Entities.Social;
using Nillero.Core.Domain.Interfaces.Social;
using Nillero.Infrastructure.Persistence.Contexts;
using Nillero.Infrastructure.Persistence.Repositories.Base;

namespace Nillero.Infrastructure.Persistence.Repositories.Social
{
    public class FriendRequestRepository : GenericRepository<FriendRequest>, IFriendRequestRepository
    {
        public FriendRequestRepository(NilleroAppContext context) : base(context)
        {

        }
    }
}
