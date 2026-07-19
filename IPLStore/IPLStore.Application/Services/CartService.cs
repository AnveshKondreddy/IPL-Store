using IPLStore.Application.Interfaces;
using IPLStore.Application.Interfaces.Repo;
using IPLStore.Application.Interfaces.Service;
using IPLStore.Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Services
{
    public class CartService(ICartRepository cartRepo, IProductQueries productQueries, ILogger<CartService> logger) : ICartService
    {
        public async Task<CartDto> GetCartAsync(string userId, CancellationToken cancellationToken)
        {
            var cart = await cartRepo.GetOrCreateCartAsync(userId, cancellationToken);

            var itemDtos = cart.Items.Select(i => new CartItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.Quantity,
                i.UnitPrice,
                i.Quantity * i.UnitPrice)).ToList();

            return new CartDto(userId, itemDtos, cart.TotalAmount);
        }

        public async Task<Result<CartDto>> UpsertItemAsync(
            string userId,
            UpsertCartItemRequest request,
            CancellationToken cancellationToken)
        {
            var product = await productQueries.GetProductByIdAsync(request.ProductId, cancellationToken);
            if (product is null)
                return Result<CartDto>.NotFound("Product is not available.");

            var cart = await cartRepo.GetOrCreateCartAsync(userId, cancellationToken);
            cart.AddOrUpdateItem(request.ProductId, request.Quantity, product.Price);

            await cartRepo.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Cart updated for user {UserId}", userId);
            return Result<CartDto>.Success(await GetCartAsync(userId, cancellationToken));
        }

        public async Task<Result<CartDto>> UpdateItemAsync(
            string userId,
            int productId,
            UpdateCartItemRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Quantity <= 0)
            {
                return Result<CartDto>.ValidationError("Quantity must be greater than zero.");
            }

            var cart = await cartRepo.GetOrCreateCartAsync(userId, cancellationToken);
            var item = cart.Items.SingleOrDefault(i => i.ProductId == productId);
            if (item is null)
                return Result<CartDto>.NotFound("Cart item not found.");

            item.Quantity = request.Quantity;

            await cartRepo.SaveChangesAsync(cancellationToken);
            return Result<CartDto>.Success(await GetCartAsync(userId, cancellationToken));
        }

        public async Task<CartDto> RemoveItemAsync(string userId, int productId, CancellationToken cancellationToken)
        {
            var cart = await cartRepo.GetOrCreateCartAsync(userId, cancellationToken);
            var item = cart.Items.SingleOrDefault(i => i.ProductId == productId);
            if (item is not null)
                cart.Items.Remove(item);

            await cartRepo.SaveChangesAsync(cancellationToken);
            return await GetCartAsync(userId, cancellationToken);
        }
    }
}
