using IPLStore.API.Extensions;
using IPLStore.Application.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace IPLStore.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController(IOrderService orderService) : ControllerBase
    {
        [HttpPost("checkout/{userId}")]
        public async Task<IActionResult> Checkout(
            string userId,
            CancellationToken cancellationToken)
        {
            var result = await orderService.CheckoutAsync(userId, cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetOrderHistory(
            string userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var orders = await orderService.GetOrderHistoryAsync(userId, page, pageSize, cancellationToken);
            return Ok(orders);
        }

        [HttpGet("{userId}/{orderId:int}")]
        public async Task<IActionResult> GetOrderById(string userId, int orderId, CancellationToken cancellationToken)
        {
            var order = await orderService.GetOrderByIdAsync(userId, orderId, cancellationToken);
            if (order is null)
            {
                return NotFound(new { message = "Order not found." });
            }

            return Ok(order);
        }
    }
}
