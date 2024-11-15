using Microsoft.AspNetCore.Mvc;
using Moq;
using net_inermediate.Controllers;
using net_intermediate.Models;
using net_intermediate.Repositories;

namespace net_intermediate.uTests.Controllers
{
    public class PaymentsControllerTests
    {
        private readonly PaymentsController _controller;
        private readonly Mock<IPaymentRepository> _mockRepo;

        public PaymentsControllerTests()
        {
            _mockRepo = new Mock<IPaymentRepository>();
            _controller = new PaymentsController(_mockRepo.Object);
        }
        [Fact]
        public async Task GetPayment_ReturnsNotFound_WhenPaymentDoesNotExist()
        {
            var paymentId = Guid.NewGuid();
            _mockRepo.Setup(x => x.GetPaymentAsync(paymentId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Payment)null);

            var result = await _controller.GetPayment(paymentId, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetPayment_ReturnsOkObjectResult_WithPayment_WhenPaymentExists()
        {
            var paymentId = Guid.NewGuid();
            var expectedPayment = new Payment { PaymentId = paymentId };
            _mockRepo.Setup(x => x.GetPaymentAsync(paymentId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedPayment);

            var result = await _controller.GetPayment(paymentId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedPayment, okResult.Value);
        }
        [Fact]
        public async Task CompletePayment_CallsRepository_WithCorrectParameters()
        {
            var paymentId = Guid.NewGuid();
            _mockRepo.Setup(x => x.UpdatePaymentAndSeatsStatusAsync(paymentId, "Complete", "Sold", It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            await _controller.CompletePayment(paymentId, CancellationToken.None);

            _mockRepo.Verify(x => x.UpdatePaymentAndSeatsStatusAsync(paymentId, "Complete", "Sold", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CompletePayment_ReturnsOkResult()
        {
            var paymentId = Guid.NewGuid();
            _mockRepo.Setup(x => x.UpdatePaymentAndSeatsStatusAsync(paymentId, "Complete", "Sold", It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            var result = await _controller.CompletePayment(paymentId, CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task FailPayment_CallsRepository_WithCorrectParameters()
        {
            var paymentId = Guid.NewGuid();
            _mockRepo.Setup(x => x.UpdatePaymentAndSeatsStatusAsync(paymentId, "Failed", "Available", It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            await _controller.FailPayment(paymentId, CancellationToken.None);

            _mockRepo.Verify(x => x.UpdatePaymentAndSeatsStatusAsync(paymentId, "Failed", "Available", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FailPayment_ReturnsOkResult()
        {
            var paymentId = Guid.NewGuid();
            _mockRepo.Setup(x => x.UpdatePaymentAndSeatsStatusAsync(paymentId, "Failed", "Available", It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            var result = await _controller.FailPayment(paymentId, CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }
    }
}
