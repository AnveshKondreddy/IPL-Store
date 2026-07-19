using IPLStore.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Interfaces
{
    public interface IProductQueries
    {
        Task<List<ProductListItemDto>> SearchProductsAsync(
        string? search,
        string? type,
        string? franchise,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

        Task<int> GetProductCountAsync(
            string? search,
            string? type,
            string? franchise,
            CancellationToken cancellationToken);

        Task<ProductDetailsDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken);
    }
}
