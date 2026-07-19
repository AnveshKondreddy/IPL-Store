using IPLStore.Application.Interfaces.Repo;
using IPLStore.Application.Models;
using IPLStore.Application.Services;
using IPLStore.Domain;
using Microsoft.Extensions.Logging;
using Moq;

namespace IPLStore.Tests
{
    [TestClass]
    public sealed class OrderServiceTests
    {
        private Mock<ICartRepository> _mockCartRepo = null!;
        private Mock<IOrderQueries> _mockOrderQueries = null!;
        private Mock<IOrderRepository> _mockOrderRepo = null!;
        private OrderService _sut = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockCartRepo = new Mock<ICartRepository>();
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockOrderQueries = new Mock<IOrderQueries>();
            var logger = Mock.Of<ILogger<OrderService>>();
            _sut = new OrderService(_mockCartRepo.Object, _mockOrderRepo.Object, _mockOrderQueries.Object, logger);
        }

        private static Cart CreateCartWithItems(string userId = "user1") => new()
        {
            Id = 1,
            UserId = userId,
            Items =
            [
                new CartItem
                {
                    Id = 10,
                    ProductId = 1,
                    Product = new Product { Id = 1, Name = "Mumbai Indians Match Jersey", Type = "Jersey", Description = "Official jersey", IsActive = true, StockQty = 50, Price = 2499m },
                    Quantity = 2,
                    UnitPrice = 2499m
                }
            ]
        };

        private static Cart CreateEmptyCart(string userId = "user1") => new()
        {
            Id = 1,
            UserId = userId
        };

        private static Product CreateActiveProduct(int id = 1, int stockQty = 50) => new()
        {
            Id = id,
            Name = "Mumbai Indians Match Jersey",
            Type = "Jersey",
            Description = "Official jersey",
            IsActive = true,
            StockQty = stockQty,
            Price = 2499m
        };

        // ── CheckoutAsync ──

        [TestMethod]
        public async Task CheckoutAsync_ValidCart_ReturnsSuccessAndClearsCart()
        {
            var cart = CreateCartWithItems();
            var product = CreateActiveProduct();
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);
            _mockOrderRepo.Setup(r => r.GetProductForUpdateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _sut.CheckoutAsync("user1", CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, cart.Items.Count);
            Assert.AreEqual(48, product.StockQty);
            _mockOrderRepo.Verify(r => r.AddOrderAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockOrderRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task CheckoutAsync_ValidCart_ReturnsCorrectOrderDto()
        {
            var cart = CreateCartWithItems();
            var product = CreateActiveProduct();
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);
            _mockOrderRepo.Setup(r => r.GetProductForUpdateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _sut.CheckoutAsync("user1", CancellationToken.None);

            Assert.IsNotNull(result.Value);
            Assert.AreEqual("user1", result.Value.UserId);
            Assert.AreEqual(2499m * 2, result.Value.TotalAmount);
            Assert.AreEqual(1, result.Value.Items.Count);
        }

        [TestMethod]
        public async Task CheckoutAsync_EmptyCart_ReturnsValidationError()
        {
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateEmptyCart());

            var result = await _sut.CheckoutAsync("user1", CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorKind.Validation, result.ErrorKind);
            _mockOrderRepo.Verify(r => r.AddOrderAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task CheckoutAsync_ProductNoLongerActive_ReturnsValidationError()
        {
            var cart = CreateCartWithItems();
            var inactiveProduct = CreateActiveProduct();
            inactiveProduct.IsActive = false;

            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);
            _mockOrderRepo.Setup(r => r.GetProductForUpdateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(inactiveProduct);

            var result = await _sut.CheckoutAsync("user1", CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorKind.Validation, result.ErrorKind);
        }

        [TestMethod]
        public async Task CheckoutAsync_ProductNull_ReturnsValidationError()
        {
            var cart = CreateCartWithItems();
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);
            _mockOrderRepo.Setup(r => r.GetProductForUpdateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _sut.CheckoutAsync("user1", CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorKind.Validation, result.ErrorKind);
        }

        [TestMethod]
        public async Task CheckoutAsync_InsufficientStock_ReturnsValidationError()
        {
            var cart = CreateCartWithItems();
            var lowStockProduct = CreateActiveProduct(stockQty: 1);

            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);
            _mockOrderRepo.Setup(r => r.GetProductForUpdateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lowStockProduct);

            var result = await _sut.CheckoutAsync("user1", CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorKind.Validation, result.ErrorKind);
        }

        [TestMethod]
        public async Task CheckoutAsync_ConcurrentStockUpdate_ReturnsConflict()
        {
            var cart = CreateCartWithItems();
            var product = CreateActiveProduct();
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);
            _mockOrderRepo.Setup(r => r.GetProductForUpdateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            _mockOrderRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));

            var result = await _sut.CheckoutAsync("user1", CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorKind.Conflict, result.ErrorKind);
        }

        // ── GetOrderHistoryAsync ──

        [TestMethod]
        public async Task GetOrderHistoryAsync_ReturnsPagedResult()
        {
            var orders = new List<OrderDto>
            {
                new(1, "user1", DateTimeOffset.UtcNow, 4998m, [new OrderItemDto(1, "Jersey", 2, 2499m, 4998m)])
            };
            _mockOrderQueries.Setup(q => q.GetOrdersAsync("user1", 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(orders);
            _mockOrderQueries.Setup(q => q.GetOrderCountAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _sut.GetOrderHistoryAsync("user1", 1, 10, CancellationToken.None);

            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual(1, result.Page);
            Assert.AreEqual(10, result.PageSize);
            Assert.AreEqual(1, result.TotalCount);
        }

        [TestMethod]
        public async Task GetOrderHistoryAsync_ClampsPageSize()
        {
            _mockOrderQueries.Setup(q => q.GetOrdersAsync("user1", 1, 100, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _mockOrderQueries.Setup(q => q.GetOrderCountAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await _sut.GetOrderHistoryAsync("user1", 0, 200, CancellationToken.None);

            Assert.AreEqual(1, result.Page);
            Assert.AreEqual(100, result.PageSize);
        }

        // ── GetOrderByIdAsync ──

        [TestMethod]
        public async Task GetOrderByIdAsync_ExistingOrder_ReturnsOrder()
        {
            var order = new OrderDto(1, "user1", DateTimeOffset.UtcNow, 4998m,
                [new OrderItemDto(1, "Jersey", 2, 2499m, 4998m)]);
            _mockOrderQueries.Setup(q => q.GetOrderByIdAsync("user1", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var result = await _sut.GetOrderByIdAsync("user1", 1, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual(4998m, result.TotalAmount);
        }

        [TestMethod]
        public async Task GetOrderByIdAsync_NonExistingOrder_ReturnsNull()
        {
            _mockOrderQueries.Setup(q => q.GetOrderByIdAsync("user1", 999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrderDto?)null);

            var result = await _sut.GetOrderByIdAsync("user1", 999, CancellationToken.None);

            Assert.IsNull(result);
        }
    }
}
