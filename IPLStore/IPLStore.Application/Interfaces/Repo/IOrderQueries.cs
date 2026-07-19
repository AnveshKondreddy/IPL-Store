using IPLStore.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Interfaces.Repo
{
    public interface IOrderQueries
    {
        Task<List<OrderDto>> GetOrdersAsync(string userId, int page, int pageSize, CancellationToken cancellationToken);
        Task<int> GetOrderCountAsync(string userId, CancellationToken cancellationToken);
        Task<OrderDto?> GetOrderByIdAsync(string userId, int orderId, CancellationToken cancellationToken);
    }
}
