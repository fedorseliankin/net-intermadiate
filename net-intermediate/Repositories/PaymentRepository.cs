using Microsoft.EntityFrameworkCore;
using net_intermediate.Models;

namespace net_intermediate.Repositories
{
    public interface IPaymentRepository
    {
        SeatStatus ConvertStringToSeatStatus(string status);
        Task<Payment> GetPaymentAsync(string paymentId, CancellationToken ct);
        Task UpdatePaymentAndSeatsStatusAsync(string paymentId, string paymentStatus, string seatStatus, CancellationToken ct);

    }
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ITicketingContext _context;

        public PaymentRepository(ITicketingContext context)
        {
            _context = context;
        }
        public SeatStatus ConvertStringToSeatStatus(string status)
        {
            if (Enum.TryParse<SeatStatus>(status, out var seatStatus))
            {
                return seatStatus;
            }
            else
            {
                throw new ArgumentException($"Invalid seat status value: {status}");
            }
        }
        public async Task<Payment> GetPaymentAsync(string paymentId, CancellationToken ct)
        {
            return await _context.Payments
                .Include(p => p.Seats)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct);
        }

        public async Task UpdatePaymentAndSeatsStatusAsync(string paymentId, string paymentStatus, string seatStatus, CancellationToken ct)
        {
            var payment = await GetPaymentAsync(paymentId, ct);
            if (payment == null)
                throw new KeyNotFoundException("Payment not found.");

            payment.Status = paymentStatus;

            var seatStatusEnum = ConvertStringToSeatStatus(seatStatus);

            if (payment.Seats != null)
            {
                foreach (var seat in payment.Seats)
                {
                    seat.Status = seatStatusEnum;
                    _context.Update(seat);
                }
            }

            _context.Update(payment);
            await _context.SaveChangesAsync(ct);
        }
    }
}
