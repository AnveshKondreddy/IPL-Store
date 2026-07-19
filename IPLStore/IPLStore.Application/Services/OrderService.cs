using IPLStore.Application.Interfaces.Repo;
using IPLStore.Application.Interfaces.Service;
using IPLStore.Application.Models;
using IPLStore.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Services
{
    public class OrderService(ICartRepository cartRepo, IOrderRepository orderRepo, IOrderQueries orderQueries, ILogger<OrderService> logger) : IOrderService
    {
        public async Task<Result<OrderDto>> CheckoutAsync(string userId, CancellationToken cancellationToken)
        {
            var cart = await cartRepo.GetOrCreateCartAsync(userId, cancellationToken);

            if (cart.Items.Count == 0)
                return Result<OrderDto>.ValidationError("Cannot checkout an empty cart.");

            foreach (var item in cart.Items)
            {
                var product = await orderRepo.GetProductForUpdateAsync(item.ProductId, cancellationToken);
                if (product is null || !product.IsActive)
                    return Result<OrderDto>.ValidationError("Product is no longer available.");

                if (product.StockQty < item.Quantity)
                    return Result<OrderDto>.ValidationError("Insufficient stock for one or more items.");

                product.StockQty -= item.Quantity;
                item.UnitPrice = product.Price;
            }

            var order = Order.FormOrderFromCart(userId, cart);
            await orderRepo.AddOrderAsync(order, cancellationToken);

            cart.Items.Clear();

            try
            {
                await orderRepo.SaveChangesAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return Result<OrderDto>.Conflict("Checkout could not be completed due to concurrent stock updates. Please retry.");
            }

            logger.LogInformation("Order {OrderId} created for user {UserId}, total={TotalAmount}", order.Id, userId, order.TotalAmount);
            return Result<OrderDto>.Success(Map(order));
        }

        public async Task<PagedResult<OrderDto>> GetOrderHistoryAsync(
            string userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var safePage = Math.Max(page, 1);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            var orders = await orderQueries.GetOrdersAsync(userId, safePage, safePageSize, cancellationToken);
            var totalCount = await orderQueries.GetOrderCountAsync(userId, cancellationToken);

            return new PagedResult<OrderDto>(orders, safePage, safePageSize, totalCount);
        }

        public Task<OrderDto?> GetOrderByIdAsync(string userId, int orderId, CancellationToken cancellationToken)
            => orderQueries.GetOrderByIdAsync(userId, orderId, cancellationToken);

        private static OrderDto Map(Order order)
        {
            var items = order.Items.Select(i => new OrderItemDto(
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity)).ToList();

            return new OrderDto(order.Id, order.UserId, order.OrderedAt, order.TotalAmount, items);
        }
    }
}
