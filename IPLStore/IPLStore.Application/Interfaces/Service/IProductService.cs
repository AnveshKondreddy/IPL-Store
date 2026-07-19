using IPLStore.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<ProductListItemDto>> SearchAsync(string? search, string? type, string? franchise, int page, int pageSize, CancellationToken cancellationToken);
        Task<ProductDetailsDto?> GetByIdAsync(int productId, CancellationToken cancellationToken);
    }
}