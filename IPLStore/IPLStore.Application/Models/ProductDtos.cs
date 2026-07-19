using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Models
{
    public record ProductListItemDto(int Id, string Name, string Type, decimal Price, string Franchise);

    public record ProductDetailsDto(
        int Id,
        string Name,
        string Type,
        string Description,
        decimal Price,
        int StockQty,
        string Franchise);
}
