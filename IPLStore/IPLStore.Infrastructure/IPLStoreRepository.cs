using IPLStore.Application.Interfaces;
using IPLStore.Application.Interfaces.Repo;
using IPLStore.Application.Models;
using IPLStore.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Infrastructure
{
    public class IPLStoreRepository(IPLStoreDbContext dbContext) : IProductQueries, ICartRepository, IOrderRepository, IOrderQueries
    {
        public async Task<Cart> GetOrCreateCartAsync(string userId, CancellationToken cancellationToken)
        {
            var cart = await dbContext.Carts
                .AsSplitQuery()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .SingleOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart is not null)
            {
                return cart;
            }

            cart = new Cart { UserId = userId };
            dbContext.Carts.Add(cart);
            return cart;
        }

        public Task<List<OrderDto>> GetOrdersAsync(string userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var safePage = Math.Max(page, 1);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            return dbContext.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderedAt)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(o => new OrderDto(
                    o.Id,
                    o.UserId,
                    o.OrderedAt,
                    o.TotalAmount,
                    o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.UnitPrice * i.Quantity)).ToList()))
                .ToListAsync(cancellationToken);
        }

        public Task<int> GetOrderCountAsync(string userId, CancellationToken cancellationToken)
            => dbContext.Orders.CountAsync(o => o.UserId == userId, cancellationToken);

        public Task<OrderDto?> GetOrderByIdAsync(string userId, int orderId, CancellationToken cancellationToken)
            => dbContext.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId && o.Id == orderId)
                .Select(o => new OrderDto(
                    o.Id,
                    o.UserId,
                    o.OrderedAt,
                    o.TotalAmount,
                    o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.UnitPrice * i.Quantity)).ToList()))
                .SingleOrDefaultAsync(cancellationToken);

        public async Task<List<ProductListItemDto>> SearchProductsAsync(
            string? search,
            string? type,
            string? franchise,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var safePage = Math.Max(page, 1);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            return await ApplyProductFilters(search, type, franchise)
                .OrderBy(p => p.Id)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(p => new ProductListItemDto(
                    p.Id,
                    p.Name,
                    p.Type,
                    p.Price,
                    p.Franchise != null ? p.Franchise.Name : string.Empty))
                .ToListAsync(cancellationToken);
        }

        public Task<int> GetProductCountAsync(
            string? search,
            string? type,
            string? franchise,
            CancellationToken cancellationToken)
            => ApplyProductFilters(search, type, franchise).CountAsync(cancellationToken);

        public Task<ProductDetailsDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
            => dbContext.Products
                .AsNoTracking()
                .Where(p => p.Id == productId && p.IsActive)
                .Select(p => new ProductDetailsDto(
                    p.Id,
                    p.Name,
                    p.Type,
                    p.Description,
                    p.Price,
                    p.StockQty,
                    p.Franchise != null ? p.Franchise.Name : string.Empty))
                .SingleOrDefaultAsync(cancellationToken);

        public Task<Product?> GetProductForUpdateAsync(int productId, CancellationToken cancellationToken)
            => dbContext.Products.SingleOrDefaultAsync(p => p.Id == productId, cancellationToken);

        public Task AddOrderAsync(Order order, CancellationToken cancellationToken)
        {
            dbContext.Orders.Add(order);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("A concurrency conflict occurred. Please retry.");
            }
        }

        private IQueryable<Product> ApplyProductFilters(string? search, string? type, string? franchise)
        {
            var query = dbContext.Products
                .AsNoTracking()
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchValue = search.Trim();
                query = query.Where(p => p.Name.Contains(searchValue) || p.Description.Contains(searchValue));
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                var typeValue = type.Trim();
                query = query.Where(p => p.Type == typeValue);
            }

            if (!string.IsNullOrWhiteSpace(franchise))
            {
                var franchiseValue = franchise.Trim();
                query = query.Where(p => p.Franchise != null && p.Franchise.Code == franchiseValue);
            }

            return query;
        }
    }
}
