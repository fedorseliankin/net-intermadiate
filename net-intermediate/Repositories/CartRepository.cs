using Microsoft.EntityFrameworkCore;
using net_intermediate.Models;

namespace net_intermediate.Repositories
{
    public interface ICartRepository
    {
        Task<Cart> GetCartAsync(Guid cartId, CancellationToken ct);
        Task AddToCartAsync(Guid cartId, CartItem item, CancellationToken ct);
        Task RemoveFromCartAsync(Guid cartId, int eventId, int seatId, CancellationToken ct);
        Task ClearCartAsync(Guid cartId, CancellationToken ct);

    }
    public class CartRepository : ICartRepository
    {
        private readonly ITicketingContext _context;

        public CartRepository(ITicketingContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartAsync(Guid cartId, CancellationToken ct)
        {
            return await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Event)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Seat)
                .Include(c => c.Items)
                    .ThenInclude(i => i.PriceOption)
                .FirstOrDefaultAsync(c => c.CartId == cartId, ct);
        }

        public async Task AddToCartAsync(Guid cartId, CartItem item, CancellationToken ct)
        {
            var cart = await _context.Carts.FindAsync(cartId, ct);
            if (cart == null)
            {
                cart = new Cart { CartId = cartId };
                await _context.Carts.AddAsync(cart, ct);
                await _context.SaveChangesAsync(ct);
            }

            item.CartId = cartId;
            await _context.CartItems.AddAsync(item, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task RemoveFromCartAsync(Guid cartId, int eventId, int seatId, CancellationToken ct)
        {
            var item = await _context.CartItems
                .Where(ci => ci.CartId == cartId && ci.EventId == eventId && ci.SeatId == seatId)
                .FirstOrDefaultAsync(ct);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task ClearCartAsync(Guid cartId, CancellationToken ct)
        {
            var cart = await GetCartAsync(cartId, ct);
            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
