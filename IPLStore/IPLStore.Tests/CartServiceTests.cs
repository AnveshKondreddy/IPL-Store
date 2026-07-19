using IPLStore.Application.Interfaces;
using IPLStore.Application.Interfaces.Repo;
using IPLStore.Application.Models;
using IPLStore.Application.Services;
using IPLStore.Domain;
using Microsoft.Extensions.Logging;
using Moq;

namespace IPLStore.Tests
{
    [TestClass]
    public sealed class CartServiceTests
    {
        private static readonly ProductDetailsDto TestProduct = new(
            1, "Mumbai Indians Match Jersey", "Jersey",
            "Official blue match jersey.", 2499m, 50, "Mumbai Indians");

        private Mock<ICartRepository> _mockCartRepo = null!;
        private Mock<IProductQueries> _mockProductQueries = null!;
        private CartService _sut = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockCartRepo = new Mock<ICartRepository>();
            _mockProductQueries = new Mock<IProductQueries>();
            var logger = Mock.Of<ILogger<CartService>>();
            _sut = new CartService(_mockCartRepo.Object, _mockProductQueries.Object, logger);
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
                    Product = new Product { Id = 1, Name = "Mumbai Indians Match Jersey", Type = "Jersey", Description = "Official jersey" },
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

        // ── GetCartAsync ──

        [TestMethod]
        public async Task GetCartAsync_WithItems_ReturnsCartDto()
        {
            var cart = CreateCartWithItems();
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var result = await _sut.GetCartAsync("user1", CancellationToken.None);

            Assert.AreEqual("user1", result.UserId);
            Assert.AreEqual(1, result.Items.Count);
            Assert.AreEqual("Mumbai Indians Match Jersey", result.Items[0].ProductName);
            Assert.AreEqual(2, result.Items[0].Quantity);
            Assert.AreEqual(2499m * 2, result.TotalAmount);
        }

        [TestMethod]
        public async Task GetCartAsync_EmptyCart_ReturnsZeroTotal()
        {
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateEmptyCart());

            var result = await _sut.GetCartAsync("user1", CancellationToken.None);

            Assert.AreEqual(0, result.Items.Count);
            Assert.AreEqual(0m, result.TotalAmount);
        }

        // ── UpsertItemAsync ──

        [TestMethod]
        public async Task UpsertItemAsync_ValidProduct_ReturnsSuccess()
        {
            var cart = CreateEmptyCart();
            _mockProductQueries.Setup(q => q.GetProductByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestProduct);
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var request = new UpsertCartItemRequest(1, 3);
            var result = await _sut.UpsertItemAsync("user1", request, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            _mockCartRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(1, cart.Items.Count);
            Assert.AreEqual(3, cart.Items[0].Quantity);
        }

        [TestMethod]
        public async Task UpsertItemAsync_ProductNotFound_ReturnsNotFound()
        {
            _mockProductQueries.Setup(q => q.GetProductByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductDetailsDto?)null);

            var request = new UpsertCartItemRequest(999, 1);
            var result = await _sut.UpsertItemAsync("user1", request, CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorKind.NotFound, result.ErrorKind);
            _mockCartRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task UpsertItemAsync_ExistingItem_UpdatesQuantity()
        {
            var cart = CreateCartWithItems();
            _mockProductQueries.Setup(q => q.GetProductByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestProduct);
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var request = new UpsertCartItemRequest(1, 5);
            var result = await _sut.UpsertItemAsync("user1", request, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, cart.Items.Count);
            Assert.AreEqual(5, cart.Items[0].Quantity);
        }

        // ── UpdateItemAsync ──

        [TestMethod]
        public async Task UpdateItemAsync_ValidItem_ReturnsSuccess()
        {
            var cart = CreateCartWithItems();
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var request = new UpdateCartItemRequest(4);
            var result = await _sut.UpdateItemAsync("user1", 1, request, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(4, cart.Items[0].Quantity);
            _mockCartRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateItemAsync_ZeroQuantity_ReturnsValidationError()
        {
            var request = new UpdateCartItemRequest(0);
            var result = await _sut.UpdateItemAsync("user1", 1, request, CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorKind.Validation, result.ErrorKind);
        }

        [TestMethod]
        public async Task UpdateItemAsync_NegativeQuantity_ReturnsValidationError()
        {
            var request = new UpdateCartItemRequest(-1);
            var result = await _sut.UpdateItemAsync("user1", 1, request, CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorKind.Validation, result.ErrorKind);
        }

        [TestMethod]
        public async Task UpdateItemAsync_ItemNotInCart_ReturnsNotFound()
        {
            var cart = CreateCartWithItems();
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var request = new UpdateCartItemRequest(2);
            var result = await _sut.UpdateItemAsync("user1", 999, request, CancellationToken.None);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorKind.NotFound, result.ErrorKind);
        }

        // ── RemoveItemAsync ──

        [TestMethod]
        public async Task RemoveItemAsync_ExistingItem_RemovesFromCart()
        {
            var cart = CreateCartWithItems();
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var result = await _sut.RemoveItemAsync("user1", 1, CancellationToken.None);

            Assert.AreEqual(0, cart.Items.Count);
            _mockCartRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task RemoveItemAsync_NonExistingItem_DoesNotThrow()
        {
            var cart = CreateCartWithItems();
            _mockCartRepo.Setup(r => r.GetOrCreateCartAsync("user1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var result = await _sut.RemoveItemAsync("user1", 999, CancellationToken.None);

            Assert.AreEqual(1, cart.Items.Count);
            _mockCartRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
