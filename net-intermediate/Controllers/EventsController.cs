using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using net_intermediate.Models;
using net_intermediate.Repositories;

namespace net_intermediate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMemoryCache _memoryCache;

        public EventsController(IMemoryCache memoryCache, IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
            _memoryCache = memoryCache;
        }

        // GET: /events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents(CancellationToken ct)
        {
            var cacheKey = "Events_List";
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Event> eventList))
            {
                eventList = await _eventRepository.ListAsync(ct);
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, eventList, cacheEntryOptions);
            }

            return Ok(eventList);
        }

        // GET: /events/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id, CancellationToken ct)
        {
            var cacheKey = $"Event_{id}";
            if (!_memoryCache.TryGetValue(cacheKey, out Event eventItem))
            {
                eventItem = await _eventRepository.GetAsync(id, ct);
                if (eventItem == null)
                {
                    return NotFound();
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));
                _memoryCache.Set(cacheKey, eventItem, cacheEntryOptions);
            }

            return Ok(eventItem);
        }
    }
}
