using Microsoft.EntityFrameworkCore;
using net_intermediate.Models;

namespace net_intermediate.Repositories
{
    public class CartItemRequest
    {
        public string CartId { get; set; }
        public string SeatId { get; set; }
        public string EventId { get; set; }
        public string PriceOptionId { get; set; }
    }
    public interface ICartRepository
    {
        Task<Cart> GetCartAsync(string cartId, CancellationToken ct);
        Task AddToCartAsync(string cartId, CartItemRequest item, CancellationToken ct);
        Task RemoveFromCartAsync(string cartId, string eventId, string seatId, CancellationToken ct);
        Task ClearCartAsync(string cartId, CancellationToken ct);
        Task<bool> SeatIsBooked(string seatId, string eventId, CancellationToken ct);
    }
    public class CartRepository : ICartRepository
    {
        private readonly ITicketingContext _context;
        private readonly IDatabaseTransactionManager _transactionManager;

        public CartRepository(ITicketingContext context, IDatabaseTransactionManager transactionManager)
        {
            _context = context;
            _transactionManager = transactionManager;
        }
        public async Task<bool> SeatIsBooked(string seatId, string eventId, CancellationToken ct)
        {
            return await _context.CartItems.AnyAsync(item => item.SeatId == seatId && item.EventId == eventId, ct);
        }
        public async Task<Cart> GetCartAsync(string cartId, CancellationToken ct)
        {
            return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Event)
            .Include(c => c.Items)
                .ThenInclude(i => i.Seat)
            .Include(c => c.Items)
                .ThenInclude(i => i.PriceOption)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CartId == cartId, ct);
        }

        public async Task AddToCartAsync(string cartId, CartItemRequest request, CancellationToken ct)
        {
            int retryCount = 0;
            bool isCompleted = false;

            while (!isCompleted && retryCount < 5)  // Allows up to 5 retries
            {
                using (var transaction = _transactionManager.BeginTransaction(System.Data.IsolationLevel.Serializable))
                {
                    try
                    {
                        var cart = await _context.Carts.FindAsync(new object[] { request.CartId }, ct);
                        if (cart == null)
                        {
                            cart = new Cart { CartId = request.CartId };
                            await _context.Carts.AddAsync(cart, ct);
                            await _context.SaveChangesAsync(ct);
                        }
                        var seatAlreadyBooked = await _context.CartItems
                           .AnyAsync(ci => ci.SeatId == request.SeatId && ci.EventId == request.EventId, ct);

                        if (seatAlreadyBooked)
                        {
                            throw new InvalidOperationException("This seat is already booked.");
                        }

                        var item = new CartItem
                        {
                            CartId = request.CartId,
                            SeatId = request.SeatId,
                            EventId = request.EventId,
                            PriceOptionId = request.PriceOptionId
                        };

                        await _context.CartItems.AddAsync(item, ct);
                        await _context.SaveChangesAsync(ct);

                        transaction.Commit();
                        isCompleted = true;
                    }
                    catch (DbUpdateException ex) when (ex.InnerException != null && ex.InnerException.Message.Contains("Deadlock"))
                    {
                        // Rollback the transaction and increment the retry counter
                        transaction.Rollback();
                        retryCount++;
                        await Task.Delay(200 * retryCount, ct);  // Exponential back-off
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;  // Rethrow any other exception which is not handled here
                    }
                }
            }

            if (!isCompleted)
            {
                throw new Exception("Failed to add item to cart after multiple retries.");
            }
        }

        public async Task RemoveFromCartAsync(string cartId, string eventId, string seatId, CancellationToken ct)
        {
            var item = await _context.CartItems
                .Where(ci => ci.CartId == cartId.ToString() && ci.EventId == eventId && ci.SeatId == seatId)
                .FirstOrDefaultAsync(ct);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task ClearCartAsync(string cartId, CancellationToken ct)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CartId == cartId);
            if (cart != null)
            {
                foreach (var item in cart.Items)
                {
                    _context.CartItems.Remove(item);
                } 
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
