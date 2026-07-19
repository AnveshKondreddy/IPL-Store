using IPLStore.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Interfaces.Repo
{
    public interface IOrderRepository
    {
        Task<Product?> GetProductForUpdateAsync(int productId, CancellationToken cancellationToken);
        Task AddOrderAsync(Order order, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
