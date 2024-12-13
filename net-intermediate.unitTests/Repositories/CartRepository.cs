using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using net_intermediate.Models;
using net_intermediate.Repositories;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

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
                new CartItem { CartId = Guid.NewGuid().ToString(), EventId = "1", SeatId = "101" },
                new CartItem { CartId = Guid.NewGuid().ToString(), EventId = "1", SeatId = "102" }
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
                var cartId = Guid.NewGuid().ToString();
                var eventEntity = new Event { Id = "1", Name = "Concert", Description = "A live concert event" };
                var priceOption = new PriceOption { Id = "1", Name = "Regular Price" };
                var seatEntity = new Seat
                {
                    RowId = "A",
                    SeatId = "101",
                    Status = SeatStatus.Booked,
                    SeatName = "A1",
                    PriceOption = priceOption,
                    Section = new Section
                    {
                        SectionId = "1",
                        SectionName = "Section1",
                        VenueId = "1",
                    },
                    SectionId = "1",
                };
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
                var mockTransactionManager = new Mock<IDatabaseTransactionManager>();
                mockTransactionManager.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>()))
                                      .Returns(Mock.Of<IDbContextTransaction>());

                _mockContext.Setup(m => m.Carts).Returns(_mockDbSetCart.Object);
                _mockContext.Setup(m => m.CartItems).Returns(_mockDbSetCartItem.Object);

                var repository = new CartRepository(context, mockTransactionManager.Object);
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
                var mockTransactionManager = new Mock<IDatabaseTransactionManager>();
                mockTransactionManager.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>()))
                                      .Returns(Mock.Of<IDbContextTransaction>());

                _mockContext.Setup(m => m.Carts).Returns(_mockDbSetCart.Object);
                _mockContext.Setup(m => m.CartItems).Returns(_mockDbSetCartItem.Object);

                var repository = new CartRepository(context, mockTransactionManager.Object);
                var result = await repository.GetCartAsync(Guid.NewGuid().ToString(), CancellationToken.None);

                Assert.Null(result);
            }
        }
        [Fact]
        public async Task AddToCartAsync_CartExists_AddsItem()
        {
            string cartId = Guid.NewGuid().ToString();
            var cart = new Cart { CartId = cartId };
            var cartItem = new CartItemRequest { CartId = cartId };

            _mockDbSetCart.Setup(x => x.FindAsync(cartId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(cart);

            _mockDbSetCartItem.Setup(x => x.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
                             .Verifiable();
            var mockTransactionManager = new Mock<IDatabaseTransactionManager>();
            mockTransactionManager.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>()))
                                  .Returns(Mock.Of<IDbContextTransaction>());

            var repository = new CartRepository(_mockContext.Object, mockTransactionManager.Object);

            await repository.AddToCartAsync(cartId, cartItem, CancellationToken.None);

            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            _mockDbSetCartItem.Verify();
            Assert.Equal(cartId, cartItem.CartId);
        }

        [Fact]
        public async Task AddToCartAsync_CartNotExists_CreatesCartAndAddsItem()
        {
            string cartId = Guid.NewGuid().ToString();
            var cartItem = new CartItemRequest { CartId = cartId };

            _mockDbSetCart.Setup(x => x.FindAsync(cartId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync((Cart)null);

            _mockDbSetCart.Setup(x => x.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
                          .Verifiable();

            _mockDbSetCartItem.Setup(x => x.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
                               .Verifiable();
            var mockTransactionManager = new Mock<IDatabaseTransactionManager>();
            mockTransactionManager.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>()))
                                  .Returns(Mock.Of<IDbContextTransaction>());

            _mockContext.Setup(m => m.Carts).Returns(_mockDbSetCart.Object);
            _mockContext.Setup(m => m.CartItems).Returns(_mockDbSetCartItem.Object);

            var repository = new CartRepository(_mockContext.Object, mockTransactionManager.Object);

            await repository.AddToCartAsync(cartId, cartItem, CancellationToken.None);

            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            _mockDbSetCart.Verify();
            _mockDbSetCartItem.Verify();
            Assert.Equal(cartId, cartItem.CartId);
        }
        [Fact]
        public async Task RemoveFromCartAsync_ItemDoesNotExist_NoActionTaken()
        {
            string nonExistingCartId = Guid.NewGuid().ToString();
            string eventId = "9999";
            string seatId = "9999";
            var mockTransactionManager = new Mock<IDatabaseTransactionManager>();
            mockTransactionManager.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>()))
                                  .Returns(Mock.Of<IDbContextTransaction>());

            _mockContext.Setup(m => m.Carts).Returns(_mockDbSetCart.Object);
            _mockContext.Setup(m => m.CartItems).Returns(_mockDbSetCartItem.Object);

            var repository = new CartRepository(_mockContext.Object, mockTransactionManager.Object);

            await repository.RemoveFromCartAsync(nonExistingCartId, eventId, seatId, CancellationToken.None);

            _mockDbSetCartItem.Verify(x => x.Remove(It.IsAny<CartItem>()), Times.Never());
            _mockContext.Verify(m => m.SaveChangesAsync(CancellationToken.None), Times.Never());
        }
        [Fact]
        public async Task ClearCartAsync_CartExists_ClearsCart()
        {
            using (var context = new TicketingContext(_options))
            {
                string cartId = Guid.NewGuid().ToString();
                var eventEntity = new Event { Id = "1", Name = "Concert", Description = "A live concert event" };
                var priceOption = new PriceOption { Id = "1", Name = "Regular Price" };
                var seatEntity = new Seat { SeatId = "1", RowId = "5", SeatName = "A1", PriceOption = priceOption, SectionId = Guid.NewGuid().ToString() };
                var cartItemId = Guid.NewGuid().ToString();
                var Items = new List<CartItem>
                    {
                        new CartItem { EventId = "1", SeatId = "1", PriceOptionId = "1", CartItemId = cartItemId  }
                    };
                var cart = new Cart
                {
                    CartId = cartId,
                    Items = Items,
                };
                context.CartItems.AddRange(Items);
                context.PriceOptions.Add(priceOption);
                context.Events.Add(eventEntity);
                context.Seats.Add(seatEntity);
                context.Carts.Add(cart);
                await context.SaveChangesAsync();
                var mockTransactionManager = new Mock<IDatabaseTransactionManager>();
                mockTransactionManager.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>()))
                                      .Returns(Mock.Of<IDbContextTransaction>());
                var repository = new CartRepository(context, mockTransactionManager.Object);

                await repository.ClearCartAsync(cartId, CancellationToken.None);

                Assert.Empty(context.CartItems);
            }
        }

        [Fact]
        public async Task ClearCartAsync_CartNotExists_NoActionTaken()
        {
            using (var context = new TicketingContext(_options))
            {
                var mockTransactionManager = new Mock<IDatabaseTransactionManager>();
                mockTransactionManager.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>()))
                                      .Returns(Mock.Of<IDbContextTransaction>());

                _mockContext.Setup(m => m.Carts).Returns(_mockDbSetCart.Object);
                _mockContext.Setup(m => m.CartItems).Returns(_mockDbSetCartItem.Object);

                var repository = new CartRepository(context, mockTransactionManager.Object);
                string nonExistentCartId = Guid.NewGuid().ToString();
                var initialItemCount = context.CartItems.Count();

                await repository.ClearCartAsync(nonExistentCartId, CancellationToken.None);

                var itemCountAfterOperation = context.CartItems.Count();
                Assert.Equal(initialItemCount, itemCountAfterOperation);
                Assert.True(true);
            }
        }

    }
}