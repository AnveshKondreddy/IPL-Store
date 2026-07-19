using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Models
{
    public record CartItemDto(int ItemId, int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

    public record CartDto(string UserId, IReadOnlyList<CartItemDto> Items, decimal TotalAmount);
}
