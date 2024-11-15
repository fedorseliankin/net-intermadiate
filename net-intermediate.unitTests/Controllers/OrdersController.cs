using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public OrdersControllerTests()
        {
            _mockRepository = new Mock<ICartRepository>();
            _controller = new OrdersController(_mockRepository.Object);
            _cancellationToken = new CancellationToken(false);
            _mockContext = new Mock<ITicketingContext>();
            _mockContext.Setup(c => c.Events).Returns(new Mock<DbSet<Event>>().Object);
        }

        [Fact]
        public async Task GetCart_ReturnsOkObjectResult_IfCartExists()
        {
            var cartId = Guid.NewGuid();
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
            var cartId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Cart)null);

            var result = await _controller.GetCart(cartId, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddToCart_ReturnsOkResult_WithUpdatedCart()
        {
            var cartId = Guid.NewGuid();
            var newItem = new CartItem { EventId = 1, SeatId = 101 };

            _mockRepository.Setup(repo => repo.AddToCartAsync(cartId, newItem, It.IsAny<CancellationToken>()))
                     .Verifiable();

            var mockCart = new Cart { CartId = cartId, Items = new List<CartItem> { newItem } };
            _mockRepository.Setup(repo => repo.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(mockCart);

            var result = await _controller.AddToCart(cartId, newItem, CancellationToken.None);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCart = Assert.IsType<Cart>(okResult.Value);

            Assert.Single(returnedCart.Items);
        }
        [Fact]
        public async Task RemoveFromCart_ReturnsOkResult_WithUpdatedCart()
        {
            var cartId = Guid.NewGuid();
            var eventId = 1;
            var seatId = 101;
            _mockRepository.Setup(repo => repo.RemoveFromCartAsync(cartId, eventId, seatId, It.IsAny<CancellationToken>()))
                     .Verifiable();

            var updatedCart = new Cart { CartId = cartId, Items = new List<CartItem>() }; // Assuming the item was removed
            _mockRepository.Setup(repo => repo.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(updatedCart);

            var result = await _controller.RemoveFromCart(cartId, eventId, seatId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCart = Assert.IsType<Cart>(okResult.Value);
            Assert.Empty(returnedCart.Items); // Expect the cart to be empty since items were removed
        }

        [Fact]
        public async Task BookCart_ReturnsOkResult_WithPaymentId()
        {
            var cartId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.ClearCartAsync(cartId, It.IsAny<CancellationToken>()))
                     .Verifiable();

            var result = await _controller.BookCart(cartId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseObject = Assert.IsType<Payment>(okResult.Value);
            Assert.NotNull(responseObject);
            Assert.True(responseObject.PaymentId is Guid); // Checks if the response has a payment Id as GUID
            _mockRepository.Verify(repo => repo.ClearCartAsync(cartId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
