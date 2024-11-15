using Microsoft.EntityFrameworkCore;
using net_intermediate.Models;

namespace net_intermediate.Repositories
{
    public interface IVenueRepository
    {
        Task<IEnumerable<Venue>> GetAllVenuesAsync(CancellationToken ct);
        Task<IEnumerable<Section>> GetSectionsByVenueIdAsync(int venueId, CancellationToken ct);
    }

    public class VenueRepository : IVenueRepository
    {
        private readonly TicketingContext _context;

        public VenueRepository(TicketingContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venue>> GetAllVenuesAsync(CancellationToken ct)
        {
            return await _context.Venues.ToListAsync(ct);
        }

        public async Task<IEnumerable<Section>> GetSectionsByVenueIdAsync(int venueId, CancellationToken ct)
        {
            return await _context.Sections
                                 .Where(s => s.VenueId == venueId)
                                 .ToListAsync(ct);
        }
    }
}
