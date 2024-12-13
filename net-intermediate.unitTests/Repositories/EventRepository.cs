using Microsoft.EntityFrameworkCore;
using Moq;
using net_intermediate.Models;
using net_intermediate.Repositories;
using net_intermediate;

namespace net_intermediate.uTests.Repositories
{
    public class EventRepositoryTests
    {
        private readonly Mock<DbSet<Event>> _mockSet;
        private readonly Mock<ITicketingContext> _mockContext;
        private readonly CancellationToken _cancellationToken;

        public EventRepositoryTests()
        {
            _mockSet = new Mock<DbSet<Event>>();
            _mockContext = new Mock<ITicketingContext>();
            _cancellationToken = new CancellationToken(false);

            _mockContext.Setup(m => m.Events).Returns(_mockSet.Object);
        }

        [Fact]
        public async Task AddAsync_AddsEvent()
        {
            var eventRepository = new EventRepository(_mockContext.Object);
            var newEvent = new Event();

            await eventRepository.AddAsync(newEvent, _cancellationToken);

            _mockSet.Verify(m => m.AddAsync(newEvent, _cancellationToken), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(_cancellationToken), Times.Once());
        }
        [Fact]
        public async Task DeleteAsync_ExistingEventId_RemovesEventAndSavesChanges()
        {
            string eventId = "1";
            Event eventToDelete = new Event { Id = eventId };
            var eventRepository = new EventRepository(_mockContext.Object);

            _mockSet.Setup(x => x.FindAsync(eventId, _cancellationToken)).ReturnsAsync(eventToDelete);

            await eventRepository.DeleteAsync(eventId, _cancellationToken);

            _mockSet.Verify(dbSet => dbSet.Remove(It.Is<Event>(e => e == eventToDelete)), Times.Once);
            _mockContext.Verify(context => context.SaveChangesAsync(_cancellationToken), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingEventId_DoesNotRemoveEventOrSaveChanges()
        {
            string nonExistingEventId = "2";
            var eventRepository = new EventRepository(_mockContext.Object);
            _mockSet.Setup(x => x.FindAsync(nonExistingEventId, _cancellationToken)).ReturnsAsync((Event)null);

            await eventRepository.DeleteAsync(nonExistingEventId, _cancellationToken);

            _mockSet.Verify(dbSet => dbSet.Remove(It.IsAny<Event>()), Times.Never);
            _mockContext.Verify(context => context.SaveChangesAsync(_cancellationToken), Times.Never);
        }
    }
}