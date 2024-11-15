namespace net_intermediate.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
    }
    public class Ticket
    {
        public int TicketId { get; set; }
        public int EventId { get; set; }
        public string SeatNumber { get; set; }
        public decimal Price { get; set; }
        public Event Event { get; set; }
    }
    public class Venue
    {
        public int VenueId { get; set; }
        public string Name { get; set; }
        public ICollection<Section> Sections { get; set; }
    }
    public class Section
    {
        public int SectionId { get; set; }
        public int VenueId { get; set; }
        public string SectionName { get; set; }

        public Venue Venue { get; set; }
    }
    public class Seat
    {
        public int SeatId { get; set; }
        public int SectionId { get; set; }
        public string RowId { get; set; }
        public string SeatName { get; set; }
        public SeatStatus Status { get; set; }
        public PriceOption PriceOption { get; set; }

        public Section Section { get; set; }
    }
    public enum SeatStatus
    {
        Available,
        Booked,
        Sold
    }
    public class PriceOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
    public class Cart
    {
        public Guid CartId { get; set; }
        public virtual List<CartItem> Items { get; set; } = new List<CartItem>();
    }
    public class CartItem
    {
        public int CartItemId { get; set; }
        public Guid CartId { get; set; } // Foreign key for Cart
        public int EventId { get; set; }
        public int SeatId { get; set; }
        public int PriceOptionId { get; set; }
        public virtual Cart Cart { get; set; }
        public virtual Event Event { get; set; }
        public virtual Seat Seat { get; set; }
        public virtual PriceOption PriceOption { get; set; }
    }
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; }
        public virtual List<Seat> Seats { get; set; } = new List<Seat>();
    }
}
