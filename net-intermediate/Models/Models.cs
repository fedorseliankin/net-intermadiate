

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace net_intermediate.Models
{
    public class Event
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateTime { get; set; }
        [JsonIgnore]
        public ICollection<Ticket> Tickets { get; set; } = null!;
    }
    public class Ticket
    {
        public string TicketId { get; set; }
        public string EventId { get; set; }
        public string SeatNumber { get; set; }
        public decimal Price { get; set; }
        [JsonIgnore]
        public Event Event { get; set; }
    }
    public class Venue
    {
        public string VenueId { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public ICollection<Section> Sections { get; set; }
    }
    public class Section
    {
        public string SectionId { get; set; }
        public string VenueId { get; set; }
        public string SectionName { get; set; }
        [JsonIgnore]
        public Venue Venue { get; set; }
    }
    public class Seat
    {
        public string SeatId { get; set; }
        public string SectionId { get; set; }
        public string RowId { get; set; }
        public string SeatName { get; set; }
        public SeatStatus Status { get; set; }
        [JsonIgnore]
        public PriceOption PriceOption { get; set; }
        [JsonIgnore]
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
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
    public class Cart
    {
        public string CartId { get; set; }
        [JsonIgnore]
        public virtual List<CartItem> Items { get; set; } = new List<CartItem>();
    }
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CartItemId { get; set; }
        public string CartId { get; set; } // Foreign key for Cart
        public string EventId { get; set; }
        public string SeatId { get; set; }
        public string PriceOptionId { get; set; }
        [JsonIgnore]
        public virtual Cart Cart { get; set; }
        [JsonIgnore]
        public virtual Event Event { get; set; }
        [JsonIgnore]
        public virtual Seat Seat { get; set; }
        [JsonIgnore]
        public virtual PriceOption PriceOption { get; set; }
    }
    public class Payment
    {
        public string PaymentId { get; set; }
        public string Status { get; set; }
        [JsonIgnore]
        public virtual List<Seat> Seats { get; set; } = new List<Seat>();
    }
}
