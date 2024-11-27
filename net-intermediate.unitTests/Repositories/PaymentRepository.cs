using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using net_intermediate.Models;
using net_intermediate.Repositories;
using net_intermediate;

namespace net_intermediate.uTests.Repositories
{
    public class PaymentRepositoryTests
    {
        private readonly DbContextOptions<TicketingContext> _options;

        public PaymentRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<TicketingContext>()
                        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                        .Options;
        }

        [Theory]
        [InlineData("Available", SeatStatus.Available)]
        [InlineData("Booked", SeatStatus.Booked)]
        [InlineData("Sold", SeatStatus.Sold)]
        public void ConvertStringToSeatStatus_ValidStatus_ReturnsCorrectEnum(string status, SeatStatus expected)
        {
            // Arrange
            var contextMock = new Mock<ITicketingContext>();
            var repo = new PaymentRepository(contextMock.Object);

            // Act
            var result = repo.ConvertStringToSeatStatus(status);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertStringToSeatStatus_InvalidStatus_ThrowsArgumentException()
        {
            var contextMock = new Mock<ITicketingContext>();
            var repo = new PaymentRepository(contextMock.Object);

            Assert.Throws<ArgumentException>(() => repo.ConvertStringToSeatStatus("InvalidStatus"));
        }
        [Fact]
        public async Task GetPaymentAsync_PaymentExists_ReturnsPayment()
        {
            using (var context = new TicketingContext(_options))
            {
                var paymentId = Guid.NewGuid().ToString();
                var payment = new Payment
                {
                    PaymentId = paymentId,
                    Seats = new List<Seat>(),
                    Status = "Booked",

                };
                context.Payments.Add(payment);
                await context.SaveChangesAsync();

                var repository = new PaymentRepository(context);
                var result = await repository.GetPaymentAsync(paymentId, CancellationToken.None);

                Assert.NotNull(result);
                Assert.Equal(paymentId, result.PaymentId);
            }
        }

        [Fact]
        public async Task UpdatePaymentAndSeatsStatusAsync_ValidUpdate_UpdatesStatus()
        {
            var options = new DbContextOptionsBuilder<TicketingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;
            var paymentId = Guid.NewGuid().ToString();

            using (var context = new TicketingContext(options))
            {
                var existingPayment = new Payment
                {
                    PaymentId = paymentId,
                    Status = "Pending",
                    Seats = new List<Seat>
                    {
                        new Seat {
                            RowId = "A",
                            SeatId = "101",
                            Status = SeatStatus.Booked,
                            SeatName = "A1",
                            PriceOption = new PriceOption
                            {
                                Id = "1",
                                Name = "Default",
                            },
                            Section = new Section
                            {
                                SectionId = "1",
                                SectionName = "Section1",
                                VenueId = "1",
                            },
                            SectionId = "1",
                        }
                    }
                };
                context.Payments.Add(existingPayment);
                await context.SaveChangesAsync();
            }

            using (var context = new TicketingContext(options))
            {
                var repo = new PaymentRepository(context);

                await repo.UpdatePaymentAndSeatsStatusAsync(paymentId, "Processed", "Available", CancellationToken.None);
            }

            using (var context = new TicketingContext(options))
            {
                var updatedPayment = await context.Payments.Include(p => p.Seats).FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                Assert.NotNull(updatedPayment);
                Assert.Equal("Processed", updatedPayment.Status);
                Assert.All(updatedPayment.Seats, seat => Assert.Equal(SeatStatus.Available, seat.Status));
            }
        }
    }
}
