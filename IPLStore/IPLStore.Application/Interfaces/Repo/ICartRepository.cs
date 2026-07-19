using IPLStore.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Interfaces.Repo
{
    public interface ICartRepository
    {
        Task<Cart> GetOrCreateCartAsync(string userId, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }

}
