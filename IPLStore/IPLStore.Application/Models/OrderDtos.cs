using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Models
{
    public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);
    public record OrderDto(int Id, string UserId, DateTimeOffset OrderedAt, decimal TotalAmount, IReadOnlyList<OrderItemDto> Items);
}
