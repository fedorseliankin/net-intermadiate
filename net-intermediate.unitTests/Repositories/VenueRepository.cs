using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using net_intermediate.Models;
using net_intermediate.Repositories;

namespace net_intermediate.uTests.Repositories
{
    public class VenueRepositoryTests
    {
        private readonly TicketingContext _context;
        private readonly VenueRepository _repository;
        public VenueRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<TicketingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TicketingContext(options);
            _repository = new VenueRepository(_context);

            _context.Venues.AddRange(new Venue { VenueId = "1", Name = "Venue1" },
                                     new Venue { VenueId = "2", Name = "Venue2" });
            _context.Sections.AddRange(new Section { SectionId = "1", VenueId = "1", SectionName = "Section1" },
                                       new Section { SectionId = "1", VenueId = "1", SectionName = "Section2" });
            _context.SaveChanges();
        }
        [Fact]
        public async Task GetAllVenuesAsync_ReturnsAllVenues()
        {
            var venues = await _repository.GetAllVenuesAsync(CancellationToken.None);

            Assert.NotNull(venues);
            Assert.Equal(2, venues.Count());
        }
        [Fact]
        public async Task GetSectionsByVenueIdAsync_ReturnsSectionsForGivenVenueId()
        {
            var sections = await _repository.GetSectionsByVenueIdAsync("1", CancellationToken.None);

            Assert.NotNull(sections);
            Assert.Equal(2, sections.Count());
        }

        [Fact]
        public async Task GetSectionsByVenueIdAsync_ReturnsEmpty_WhenNoSectionsExistForVenueId()
        {
            var sections = await _repository.GetSectionsByVenueIdAsync("999", CancellationToken.None);

            Assert.NotNull(sections);
            Assert.Empty(sections);
        }
    }
}
 