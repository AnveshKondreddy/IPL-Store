using IPLStore.API.Extentions;
using IPLStore.Application.Interfaces.Service;
using IPLStore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace IPLStore.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartsController(ICartService cartService) : ControllerBase
    {
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId, CancellationToken cancellationToken)
        {
            var cart = await cartService.GetCartAsync(userId, cancellationToken);
            return Ok(cart);
        }

        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AddItem(
            string userId,
            [FromBody] UpsertCartItemRequest request,
            CancellationToken cancellationToken)
        {
            if (request.ProductId <= 0 || request.Quantity <= 0)
                return BadRequest(new { message = "ProductId and Quantity must be positive." });

            var result = await cartService.UpsertItemAsync(userId, request, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{userId}/items/{productId:int}")]
        public async Task<IActionResult> UpdateItem(
            string userId,
            int productId,
            [FromBody] UpdateCartItemRequest request,
            CancellationToken cancellationToken)
        {
            var result = await cartService.UpdateItemAsync(userId, productId, request, cancellationToken);
            return result.ToActionResult();
        }

        [HttpDelete("{userId}/items/{productId:int}")]
        public async Task<IActionResult> RemoveItem(string userId, int productId, CancellationToken cancellationToken)
        {
            var cart = await cartService.RemoveItemAsync(userId, productId, cancellationToken);
            return Ok(cart);
        }
    }
}
