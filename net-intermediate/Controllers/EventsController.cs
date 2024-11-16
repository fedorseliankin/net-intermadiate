using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using net_intermediate.Models;
using net_intermediate.Repositories;
using System.Text;
using System.Security.Cryptography;

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
                if (eventList != null)
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                    _memoryCache.Set(cacheKey, eventList, cacheEntryOptions);
                }
            }

            if (eventList == null)
            {
                return NotFound("No events found.");
            }

            var etag = GenerateETag(eventList);
            if (HttpContext?.Response != null)
            {
                HttpContext.Response.Headers.Add("Cache-Control", "public, max-age=3600");
                HttpContext.Response.Headers.Add("ETag", etag);
                HttpContext.Response.Headers.Add("Last-Modified", DateTime.UtcNow.ToString("R"));
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

            var etag = GenerateETag(eventItem);

            if (HttpContext?.Request != null)
            {
                if (Request.Headers.ContainsKey("If-None-Match") && Request.Headers["If-None-Match"].ToString() == etag)
                {
                    return StatusCode(StatusCodes.Status304NotModified);
                }
            }

            if (HttpContext?.Response != null)
            {
                HttpContext.Response.Headers.Add("Cache-Control", "public, max-age=3600");
                HttpContext.Response.Headers.Add("ETag", etag);
                HttpContext.Response.Headers.Add("Last-Modified", DateTime.UtcNow.ToString("R"));
            }
            return Ok(eventItem);
        }

        private string GenerateETag(IEnumerable<Event> eventList)
        {
            var serializedEvents = JsonConvert.SerializeObject(eventList);
            return BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(serializedEvents)));
        }
        private string GenerateETag(Event eventItem)
        {
            if (eventItem == null)
            {
                throw new ArgumentNullException(nameof(eventItem), "Cannot generate ETag for a null event item.");
            }

            var serializedEvent = JsonConvert.SerializeObject(eventItem);
            return BitConverter.ToString(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(serializedEvent)));
        }
    }
}
