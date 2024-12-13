using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using net_intermediate.Models;
using net_intermediate.Repositories;
using Nito.AsyncEx;
using System.Collections.Concurrent;
using System.Data;
using System.Threading;

namespace net_intermediate.Controllers
{
    [Route("api/orders/carts")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly AsyncLock _mutex = new AsyncLock();


        public OrdersController(IMemoryCache memoryCache, ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
            _memoryCache = memoryCache;
        }

        [HttpGet("{cartId}")]
        public async Task<IActionResult> GetCart(string cartId, CancellationToken ct)
        {
            var cacheKey = $"Cart_{cartId}";
            if (!_memoryCache.TryGetValue(cacheKey, out Cart cart))
            {
                cart = await _cartRepository.GetCartAsync(cartId, ct);
                if (cart == null)
                    return NotFound();

                _memoryCache.Set(cacheKey, cart, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }

            return Ok(cart);
        }

        [HttpPost("{cartId}")]
        public async Task<IActionResult> AddToCart(string cartId, [FromBody] CartItemRequest item, CancellationToken ct)
        {
            try
            {
                await _cartRepository.AddToCartAsync(cartId, item, ct);
                _memoryCache.Remove($"Cart_{cartId}");

                return Ok(await _cartRepository.GetCartAsync(cartId, ct));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{cartId}/events/{eventId}/seats/{seatId}")]
        public async Task<IActionResult> RemoveFromCart(string cartId, string eventId, string seatId, CancellationToken ct)
        {
            await _cartRepository.RemoveFromCartAsync(cartId, eventId, seatId, ct);
            _memoryCache.Remove($"Cart_{cartId}");

            return Ok(await _cartRepository.GetCartAsync(cartId, ct));
        }

        [HttpPut("{cartId}/book")]
        public async Task<IActionResult> BookCart(string cartId, CancellationToken ct)
        {
            await _cartRepository.ClearCartAsync(cartId, ct);
            _memoryCache.Remove($"Cart_{cartId}");

            string paymentId = Guid.NewGuid().ToString();
            return Ok(new Payment { PaymentId = paymentId });
        }
    }
}
