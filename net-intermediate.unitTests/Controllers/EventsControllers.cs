using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using net_intermediate;
using net_intermediate.Controllers;
using net_intermediate.Models;
using net_intermediate.Repositories;
using System.Collections.Generic;

namespace net_inermediate.uTests.Controllers
{
    public class EventsControllerTests
    {
        private readonly Mock<IEventRepository> _mockRepository;
        private readonly EventsController _controller;
        private readonly CancellationToken _cancellationToken;
        private readonly Mock<ITicketingContext> _mockContext;
        private readonly Mock<IMemoryCache> _mockMemoryCache;

        public EventsControllerTests()
        {
            _mockRepository = new Mock<IEventRepository>();
            _mockMemoryCache = new Mock<IMemoryCache>(MockBehavior.Strict);
            Mock<ICacheEntry> mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);
            _mockMemoryCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny)).Returns(false);
            _controller = new EventsController(_mockMemoryCache.Object, _mockRepository.Object);
            _cancellationToken = new CancellationToken(false);
            _mockContext = new Mock<ITicketingContext>();
            _mockContext.Setup(c => c.Events).Returns(new Mock<DbSet<Event>>().Object);
        }

        [Fact]
        public async Task GetEvents_ReturnsEvents()
        {
            var events = new List<Event> { new Event(), new Event() };
            _mockRepository.Setup(r => r.ListAsync(_cancellationToken)).ReturnsAsync(events);

            var result = await _controller.GetEvents(_cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Event>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetEvent_ReturnsNotFound()
        {
            int eventId = 1;
            _mockRepository.Setup(r => r.GetAsync(eventId, _cancellationToken)).ReturnsAsync((Event)null);

            var result = await _controller.GetEvent(eventId, _cancellationToken);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetEvent_ReturnsEvent()
        {
            int eventId = 1;
            var expectedEvent = new Event { Id = eventId };
            _mockRepository.Setup(r => r.GetAsync(eventId, _cancellationToken)).ReturnsAsync(expectedEvent);

            var result = await _controller.GetEvent(eventId, _cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var eventResult = Assert.IsType<Event>(okResult.Value);
            Assert.Equal(eventId, eventResult.Id);
        }
    }
}
