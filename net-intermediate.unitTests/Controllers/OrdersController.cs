using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using net_intermediate;
using net_intermediate.Controllers;
using net_intermediate.Models;
using net_intermediate.Repositories;  

namespace net_inermediate.uTests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<ICartRepository> _mockRepository;
        private readonly OrdersController _controller;
        private readonly CancellationToken _cancellationToken;
        private readonly Mock<ITicketingContext> _mockContext;
        private readonly Mock<IMemoryCache> _mockMemoryCache;

        public OrdersControllerTests()
        {
            _mockMemoryCache = new Mock<IMemoryCache>(MockBehavior.Strict);
            Mock<ICacheEntry> mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);
            _mockMemoryCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny)).Returns(false);
            _mockRepository = new Mock<ICartRepository>();
            _controller = new OrdersController(_mockMemoryCache.Object, _mockRepository.Object);
            _cancellationToken = new CancellationToken(false);
            _mockContext = new Mock<ITicketingContext>();
            _mockContext.Setup(c => c.Events).Returns(new Mock<DbSet<Event>>().Object);
        }

        [Fact]
        public async Task GetCart_ReturnsOkObjectResult_IfCartExists()
        {
            var cartId = Guid.NewGuid().ToString();
            var mockCart = new Cart
            {
                CartId = cartId,
                Items = new List<CartItem>()
            };

            _mockRepository.Setup(repo => repo.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(mockCart);

            var result = await _controller.GetCart(cartId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCart = Assert.IsType<Cart>(okResult.Value);
            Assert.Equal(cartId, returnedCart.CartId);
        }

        [Fact]
        public async Task GetCart_ReturnsNotFound_IfCartDoesNotExist()
        {
            var cartId = Guid.NewGuid().ToString();
            _mockRepository.Setup(repo => repo.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Cart)null);

            var result = await _controller.GetCart(cartId, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddToCart_ReturnsOkResult_WithUpdatedCart()
        {
            var cartId = Guid.NewGuid().ToString();
            var newItem = new CartItem { EventId = "1", SeatId = "101" };
            var cacheKey = $"Cart_{cartId}";

            _mockRepository.Setup(repo => repo.AddToCartAsync(cartId, newItem, It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask)
                           .Verifiable("Add to cart was never called.");

            var mockCart = new Cart { CartId = cartId, Items = new List<CartItem> { newItem } };
            _mockRepository.Setup(repo => repo.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(mockCart)
                           .Verifiable("Cart was never retrieved.");

            _mockMemoryCache.Setup(cache => cache.Remove(cacheKey))
                            .Verifiable("Cache remove was never called for the cart.");

            var result = await _controller.AddToCart(cartId.ToString(), newItem, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCart = Assert.IsType<Cart>(okResult.Value);
            Assert.Single(returnedCart.Items);

            _mockRepository.Verify();
            _mockMemoryCache.Verify(cache => cache.Remove(cacheKey), Times.Once(), "Cache was not properly invalidated.");
        }
        [Fact]
        public async Task RemoveFromCart_ReturnsOkResult_WithUpdatedCart()
        {
            string cartId = Guid.NewGuid().ToString();
            string eventId = "1";
            string seatId = "101";
            var cacheKey = $"Cart_{cartId}";
            _mockRepository.Setup(repo => repo.RemoveFromCartAsync(cartId, eventId, seatId, It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
            var updatedCart = new Cart { CartId = cartId, Items = new List<CartItem>() };
            _mockRepository.Setup(repo => repo.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(updatedCart);
            _mockMemoryCache.Setup(m => m.Remove(cacheKey)).Verifiable();

            var result = await _controller.RemoveFromCart(cartId.ToString(), eventId, seatId, CancellationToken.None);

            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var returnedCart = okResult.Value as Cart;
            Assert.Empty(returnedCart.Items);
            _mockMemoryCache.Verify(m => m.Remove(cacheKey), Times.Once);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task BookCart_ReturnsOkResult_WithPaymentId()
        {
            string cartId = Guid.NewGuid().ToString();
            var expectedPayment = new Payment { PaymentId = Guid.NewGuid().ToString() };
            var cacheKey = $"Cart_{cartId}";

            _mockRepository.Setup(repo => repo.ClearCartAsync(cartId, It.IsAny<CancellationToken>()))
                           .Verifiable();

            _mockMemoryCache.Setup(cache => cache.Remove(cacheKey))
                            .Verifiable("Cache was not invalidated for the booked cart.");

            var result = await _controller.BookCart(cartId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var paymentResult = Assert.IsType<Payment>(okResult.Value);

            _mockMemoryCache.Verify(cache => cache.Remove(cacheKey), Times.Once, "Cache entry for cart was not properly removed.");

            _mockRepository.Verify(repo => repo.ClearCartAsync(cartId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
