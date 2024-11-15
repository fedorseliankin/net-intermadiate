using Microsoft.EntityFrameworkCore;
using Moq;
using net_intermediate.Models;
using net_intermediate.Repositories;

namespace net_intermediate.uTests.Repositories
{
    public class TicketingRepositoryTests
    {

        private readonly Mock<DbSet<Ticket>> _mockSet;
        private readonly Mock<ITicketingContext> _mockContext;
        private readonly CancellationToken _cancellationToken;

        public TicketingRepositoryTests()
        {
            _mockSet = new Mock<DbSet<Ticket>>();
            _mockContext = new Mock<ITicketingContext>();
            _cancellationToken = new CancellationToken(false);

            _mockContext.Setup(m => m.Tickets).Returns(_mockSet.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTicket()
        {
            var ticket = new Ticket();
            var ticketRepository = new TicketRepository(_mockContext.Object);

            await ticketRepository.AddAsync(ticket, _cancellationToken);

            _mockSet.Verify(x => x.AddAsync(ticket, _cancellationToken), Times.Once);
            _mockContext.Verify(x => x.SaveChangesAsync(_cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Get_ShouldReturnTicketById()
        {
            int ticketID = 1;
            Ticket ticket = new Ticket { TicketId = ticketID };
            var ticketRepository = new TicketRepository(_mockContext.Object);

            _mockSet.Setup(x => x.FindAsync(ticketID, _cancellationToken)).ReturnsAsync(ticket);
            var result = await ticketRepository.Get(1, _cancellationToken);

            Assert.Equal(ticket, result);
        }
        [Fact]
        public async Task Update_ShouldCallUpdateOnDbContext()
        {
            var ticket = new Ticket { TicketId = 1, EventId = 2, SeatNumber = "A1", Price = 100.00M };
            var ticketRepository = new TicketRepository(_mockContext.Object);

            await ticketRepository.Update(ticket, _cancellationToken);

            _mockContext.Verify(ctx => ctx.Update(It.Is<Ticket>(t => t == ticket)), Times.Once);
            _mockContext.Verify(ctx => ctx.SaveChangesAsync(_cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldRemoveTicket_WhenTicketExists()
        {
            var ticketId = 1;
            var ticket = new Ticket { TicketId = ticketId, EventId = 2, SeatNumber = "A1", Price = 100.00M };
            _mockSet.Setup(m => m.FindAsync(ticketId, _cancellationToken)).ReturnsAsync(ticket);
            var ticketRepository = new TicketRepository(_mockContext.Object);

            await ticketRepository.Delete(ticketId, _cancellationToken);

            _mockSet.Verify(m => m.FindAsync(ticketId, _cancellationToken), Times.Once);
            _mockContext.Verify(m => m.Tickets.Remove(ticket), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(_cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldNotRemoveTicket_WhenTicketDoesNotExist()
        {
            var ticketId = 1;
            _mockSet.Setup(m => m.FindAsync(ticketId, _cancellationToken)).ReturnsAsync((Ticket)null);
            var ticketRepository = new TicketRepository(_mockContext.Object);

            await ticketRepository.Delete(ticketId, _cancellationToken);

            _mockSet.Verify(m => m.FindAsync(ticketId, _cancellationToken), Times.Once);
            _mockContext.Verify(m => m.Tickets.Remove(It.IsAny<Ticket>()), Times.Never);
            _mockContext.Verify(m => m.SaveChangesAsync(_cancellationToken), Times.Never);
        }
    }
}