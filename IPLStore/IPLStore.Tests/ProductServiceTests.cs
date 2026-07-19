using IPLStore.Application.Interfaces;
using IPLStore.Application.Models;
using IPLStore.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace IPLStore.Tests
{
    [TestClass]
    public sealed class Products
    {
        private static readonly List<ProductListItemDto> TestProducts =
        [
            new(1, "Mumbai Indians Match Jersey", "Jersey", 2499m, "Mumbai Indians"),
            new(2, "CSK Fan Jersey", "Jersey", 2299m, "Chennai Super Kings")
        ];

        private static readonly ProductDetailsDto TestProductDetails = new(
            1, "Mumbai Indians Match Jersey", "Jersey",
            "Official blue match jersey inspired by the Mumbai Indians home kit.",
            2499m, 50, "Mumbai Indians");

        private Mock<IProductQueries> _mockQueries = null!;
        private ProductService _service = null!;

        [TestInitialize]
        public void Initialize()
        {
            _mockQueries = new Mock<IProductQueries>();
            var logger = Mock.Of<ILogger<ProductService>>();
            _service = new ProductService(_mockQueries.Object, logger);

            _mockQueries.Setup(q => q.SearchProductsAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestProducts);

            _mockQueries.Setup(q => q.GetProductCountAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestProducts.Count);

            _mockQueries.Setup(q => q.GetProductByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TestProductDetails);

            _mockQueries.Setup(q => q.GetProductByIdAsync(It.Is<int>(id => id != 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductDetailsDto?)null);
        }

        [TestMethod]
        public async Task Search_ReturnsPagedResults()
        {
            var result = await _service.SearchAsync(null, null, null, 1, 10, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Items.Count);
            Assert.AreEqual(1, result.Page);
            Assert.AreEqual(10, result.PageSize);
            Assert.AreEqual(2, result.TotalCount);
        }

        [TestMethod]
        public async Task GetById_ExistingProduct_ReturnsProduct()
        {
            var result = await _service.GetByIdAsync(1, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual("Mumbai Indians Match Jersey", result.Name);
            Assert.AreEqual(2499m, result.Price);
        }

        [TestMethod]
        public async Task GetById_NonExistingProduct_ReturnsNull()
        {
            var result = await _service.GetByIdAsync(999, CancellationToken.None);

            Assert.IsNull(result);
        }
    }
}
