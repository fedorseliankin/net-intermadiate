using Microsoft.EntityFrameworkCore;
using net_intermediate.Models;

namespace net_intermediate.Repositories
{
    public interface IEventRepository
    {
        Task AddAsync(Event eventEntity, CancellationToken cancellationToken);
        Task<Event> GetAsync(int id, CancellationToken cancellationToken);
        Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
        Task<List<Event>> ListAsync(CancellationToken cancellationToken);
    }
    public class EventRepository : IEventRepository
    {
        private readonly ITicketingContext _context;

        public EventRepository(ITicketingContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Event e, CancellationToken ct)
        {
            await _context.Events.AddAsync(e, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<Event> GetAsync(int id, CancellationToken ct)
        {
            return await _context.Events.FindAsync(id, ct);
        }

        public async Task UpdateAsync(Event e, CancellationToken ct)
        {
            _context.Entry(e).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var e = await _context.Events.FindAsync(id, ct);
            if (e != null)
            {
                _context.Events.Remove(e);
                await _context.SaveChangesAsync(ct);
            }
        }
        public async Task<List<Event>> ListAsync(CancellationToken ct)
        {
            return await _context.Events.ToListAsync(ct);
        }
    }
}
