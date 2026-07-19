using IPLStore.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Interfaces.Service
{
    public interface IOrderService
    {
        Task<Result<OrderDto>> CheckoutAsync(string userId, CancellationToken cancellationToken);
        Task<PagedResult<OrderDto>> GetOrderHistoryAsync(string userId, int page, int pageSize, CancellationToken cancellationToken);
        Task<OrderDto?> GetOrderByIdAsync(string userId, int orderId, CancellationToken cancellationToken);
    }
}
