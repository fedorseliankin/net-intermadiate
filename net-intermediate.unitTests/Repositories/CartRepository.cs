using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using MySqlX.XDevAPI.Common;
using net_intermediate.Models;
using net_intermediate.Repositories;
using net_intermediate;

namespace net_intermediate.uTests.Repositories
{
    public class CartRepositoryTests
    {
        private readonly DbContextOptions<TicketingContext> _options;
        private readonly Mock<ITicketingContext> _mockContext;
        private readonly Mock<DbSet<Cart>> _mockDbSetCart;
        private readonly Mock<DbSet<CartItem>> _mockDbSetCartItem;

        public CartRepositoryTests()
        {
            _mockContext = new Mock<ITicketingContext>();
            _mockDbSetCart = new Mock<DbSet<Cart>>();
            _mockDbSetCartItem = new Mock<DbSet<CartItem>>();
            var cartItems = new List<CartItem>
            {
                new CartItem { CartId = Guid.NewGuid(), EventId = 1, SeatId = 101 },
                new CartItem { CartId = Guid.NewGuid(), EventId = 2, SeatId = 102 }
            };

            _mockDbSetCartItem = cartItems.AsQueryable().BuildMockDbSet();

            _mockContext.Setup(m => m.Carts).Returns(_mockDbSetCart.Object);
            _mockContext.Setup(m => m.CartItems).Returns(_mockDbSetCartItem.Object);
            _options = new DbContextOptionsBuilder<TicketingContext>()
                        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                        .Options;
        }

        [Fact]
        public async Task GetCartAsync_ReturnsCart_IfExists()
        {
            using (var context = new TicketingContext(_options))
            {
                var cartId = Guid.NewGuid();
                var eventEntity = new Event { Id = 1, Name = "Concert", Description = "A live concert event" };
                var seatEntity = new Seat { SeatId = 1, RowId = "5", SeatName = "A1" };
                var priceOption = new PriceOption { Id = 1, Name = "Regular Price" };
                var cart = new Cart
                {
                    CartId = cartId,
                    Items = new List<CartItem>
                    {
                        new CartItem { Event = eventEntity, Seat = seatEntity, PriceOption = priceOption }
                    }
                };
                context.AddRange(eventEntity, seatEntity, priceOption);
                context.Carts.Add(cart);
                await context.SaveChangesAsync();

                var repository = new CartRepository(context);
                var result = await repository.GetCartAsync(cartId, CancellationToken.None);

                Assert.NotNull(result);
                Assert.Equal(cartId, result.CartId);
                Assert.NotEmpty(result.Items);
            }
        }

        [Fact]
        public async Task GetCartAsync_ReturnsNull_IfNotExists()
        {
            using (var context = new TicketingContext(_options))
            {
                var repository = new CartRepository(context);
                var result = await repository.GetCartAsync(Guid.NewGuid(), CancellationToken.None);

                Assert.Null(result);
            }
        }
        [Fact]
        public async Task AddToCartAsync_CartExists_AddsItem()
        {
            var cartId = Guid.NewGuid();
            var cart = new Cart { CartId = cartId };
            var cartItem = new CartItem();

            _mockDbSetCart.Setup(x => x.FindAsync(cartId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(cart);

            _mockDbSetCartItem.Setup(x => x.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
                               .Verifiable();

            var repository = new CartRepository(_mockContext.Object);

            await repository.AddToCartAsync(cartId, cartItem, CancellationToken.None);

            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(1));
            _mockDbSetCartItem.Verify();
            Assert.Equal(cartId, cartItem.CartId);
        }

        [Fact]
        public async Task AddToCartAsync_CartNotExists_CreatesCartAndAddsItem()
        {
            var cartId = Guid.NewGuid();
            var cartItem = new CartItem();

            _mockDbSetCart.Setup(x => x.FindAsync(cartId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Cart)null);

            _mockDbSetCart.Setup(x => x.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
                          .Verifiable();

            _mockDbSetCartItem.Setup(x => x.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
                               .Verifiable();

            var repository = new CartRepository(_mockContext.Object);

            await repository.AddToCartAsync(cartId, cartItem, CancellationToken.None);

            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            _mockDbSetCart.Verify();
            _mockDbSetCartItem.Verify();
            Assert.Equal(cartId, cartItem.CartId);
        }
        [Fact]
        public async Task RemoveFromCartAsync_ItemDoesNotExist_NoActionTaken()
        {
            var nonExistingCartId = Guid.NewGuid();
            var eventId = 9999;
            var seatId = 9999;
            var repository = new CartRepository(_mockContext.Object);

            await repository.RemoveFromCartAsync(nonExistingCartId, eventId, seatId, CancellationToken.None);

            _mockDbSetCartItem.Verify(x => x.Remove(It.IsAny<CartItem>()), Times.Never());
            _mockContext.Verify(m => m.SaveChangesAsync(CancellationToken.None), Times.Never());
        }
        [Fact]
        public async Task ClearCartAsync_CartExists_ClearsCart()
        {
            using (var context = new TicketingContext(_options))
            {
                var cartId = Guid.NewGuid();
                var eventEntity = new Event { Id = 1, Name = "Concert", Description = "A live concert event" };
                var seatEntity = new Seat { SeatId = 1, RowId = "5", SeatName = "A1" };
                var priceOption = new PriceOption { Id = 1, Name = "Regular Price" };
                var cart = new Cart
                {
                    CartId = cartId,
                    Items = new List<CartItem>
                    {
                        new CartItem { Event = eventEntity, Seat = seatEntity, PriceOption = priceOption }
                    }
                };
                context.AddRange(eventEntity, seatEntity, priceOption);
                context.Carts.Add(cart);
                await context.SaveChangesAsync();
                var repository = new CartRepository(context);

                await repository.ClearCartAsync(cartId, CancellationToken.None);

                Assert.Empty(context.CartItems);
            }
        }

        [Fact]
        public async Task ClearCartAsync_CartNotExists_NoActionTaken()
        {
            using (var context = new TicketingContext(_options))
            {
                var repository = new CartRepository(context);
                var nonExistentCartId = Guid.NewGuid();
                var initialItemCount = context.CartItems.Count();

                await repository.ClearCartAsync(nonExistentCartId, CancellationToken.None);

                var itemCountAfterOperation = context.CartItems.Count();
                Assert.Equal(initialItemCount, itemCountAfterOperation);
                Assert.True(true);
            }
        }

    }
}