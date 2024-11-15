using Microsoft.AspNetCore.Mvc;
using Moq;
using net_intermediate.Controllers;
using net_intermediate.Models;
using net_intermediate.Repositories;

namespace net_intermediate.uTests.Controllers
{
    public class VenuesControllerTests
    {
        private readonly Mock<IVenueRepository> _mockRepo;
        private readonly VenuesController _controller;

        public VenuesControllerTests()
        {
            _mockRepo = new Mock<IVenueRepository>();
            _controller = new VenuesController(_mockRepo.Object);
        }
        [Fact]
        public async Task GetVenues_ReturnsOkResult_WithListOfVenues()
        {
            var mockVenues = new List<Venue>
            {
                new Venue { VenueId = 1, Name = "Venue1" },
                new Venue { VenueId = 2, Name = "Venue2" }
            };
            _mockRepo.Setup(repo => repo.GetAllVenuesAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(mockVenues);

            var result = await _controller.GetVenues(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedVenues = Assert.IsType<List<Venue>>(okResult.Value);
            Assert.Equal(2, returnedVenues.Count);
        }
        [Fact]
        public async Task GetSectionsByVenueId_ReturnsOkResult_WithSections()
        {
            var venueId = 1;
            var mockSections = new List<Section>
            {
                new Section { SectionId = 1, VenueId = venueId, SectionName = "Section1" },
                new Section { SectionId = 2, VenueId = venueId, SectionName = "Section2" }
            };
            _mockRepo.Setup(repo => repo.GetSectionsByVenueIdAsync(venueId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(mockSections);

            var result = await _controller.GetSectionsByVenueId(venueId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSections = Assert.IsType<List<Section>>(okResult.Value);
            Assert.Equal(2, returnedSections.Count);
        }

        [Fact]
        public async Task GetSectionsByVenueId_ReturnsNotFound_WhenNoSectionsExist()
        {
            var venueId = 999;
            _mockRepo.Setup(repo => repo.GetSectionsByVenueIdAsync(venueId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Section>());

            var result = await _controller.GetSectionsByVenueId(venueId, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
