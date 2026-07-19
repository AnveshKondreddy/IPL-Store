using IPLStore.Application.Interfaces;
using IPLStore.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace IPLStore.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] string? franchise,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
        {
            var products = await productService.SearchAsync(
                search, type, franchise,
                page, pageSize, cancellationToken);
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var product = await productService.GetByIdAsync(id, cancellationToken);
            if (product is null)
            {
                return NotFound(new { message = "Product not found." });
            }

            return Ok(product);
        }
    }
}
