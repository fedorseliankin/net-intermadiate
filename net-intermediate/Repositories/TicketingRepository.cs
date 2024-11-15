using Microsoft.EntityFrameworkCore;
using net_intermediate.Models;

namespace net_intermediate.Repositories
{
    public interface ITicketingRepository
    {
        Task AddAsync(Ticket ticket, CancellationToken ct);
        Task<Ticket> Get(int id, CancellationToken ct);
        Task Update(Ticket ticket, CancellationToken ct);
        Task Delete(int ticketId, CancellationToken ct);

    }
    public class TicketRepository : ITicketingRepository
    {
        private readonly ITicketingContext _context;

        public TicketRepository(ITicketingContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Ticket ticket, CancellationToken ct)
        {
            await _context.Tickets.AddAsync(ticket, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<Ticket> Get(int id, CancellationToken ct)
        {
            return await _context.Tickets.FindAsync(id, ct);
        }

        public async Task Update(Ticket ticket, CancellationToken ct)
        {
            _context.Update(ticket);
            await _context.SaveChangesAsync(ct);
        }

        public async Task Delete(int ticketId, CancellationToken ct)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId, ct);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
