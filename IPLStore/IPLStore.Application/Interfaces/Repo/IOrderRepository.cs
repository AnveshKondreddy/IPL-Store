using IPLStore.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Interfaces.Repo
{
    public interface IOrderRepository
    {
        Task<Product?> GetProductForUpdateAsync(int productId, CancellationToken cancellationToken);
        Task<Dictionary<int, Product>> GetProductsForUpdateAsync(IEnumerable<int> productIds, CancellationToken cancellationToken);
        Task AddOrderAsync(Order order, CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
