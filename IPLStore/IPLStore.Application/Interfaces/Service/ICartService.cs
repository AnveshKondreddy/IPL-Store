using IPLStore.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPLStore.Application.Interfaces.Service
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId, CancellationToken cancellationToken);
        Task<Result<CartDto>> UpsertItemAsync(string userId, UpsertCartItemRequest request, CancellationToken cancellationToken);
        Task<Result<CartDto>> UpdateItemAsync(string userId, int productId, UpdateCartItemRequest request, CancellationToken cancellationToken);
        Task<CartDto> RemoveItemAsync(string userId, int productId, CancellationToken cancellationToken);
    }
}
