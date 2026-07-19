using IPLStore.Application.Interfaces;
using IPLStore.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Services
{
    public class ProductService(IProductQueries productQueries, ILogger<ProductService> logger) : IProductService
    {
        public async Task<PagedResult<ProductListItemDto>> SearchAsync(
                string? search,
                string? type,
                string? franchise,
                int page,
                int pageSize,
                CancellationToken cancellationToken)
        {
            var items = await productQueries.SearchProductsAsync(search, type, franchise, page, pageSize, cancellationToken);
            var totalCount = await productQueries.GetProductCountAsync(search, type, franchise, cancellationToken);

            logger.LogInformation("Product search returned {Count} results", items.Count);
            return new PagedResult<ProductListItemDto>(items, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100), totalCount);
        }

        public Task<ProductDetailsDto?> GetByIdAsync(int productId, CancellationToken cancellationToken)
            => productQueries.GetProductByIdAsync(productId, cancellationToken);
    }
}
