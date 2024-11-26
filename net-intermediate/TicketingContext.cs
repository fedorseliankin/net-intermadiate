using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using net_intermediate.Models;
using System.Data;

namespace net_intermediate
{
    public interface IDatabaseTransactionManager
    {
        IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel);
    }
    public class DatabaseTransactionManager : IDatabaseTransactionManager
    {
        private readonly ITicketingContext _context;

        public DatabaseTransactionManager(ITicketingContext context)
        {
            _context = context;
        }

        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return _context.Database.BeginTransaction(isolationLevel);
        }
    }
    public interface ITicketingContext : IDisposable
    {
        DbSet<Event> Events { get; set; }
        DbSet<Ticket> Tickets { get; set; }
        DbSet<Venue> Venues { get; set; }
        DbSet<Section> Sections { get; set; }
        DbSet<Seat> Seats { get; set; }
        DbSet<PriceOption> PriceOptions { get; set; }
        DbSet<Cart> Carts { get; set; }
        DbSet<CartItem> CartItems { get; set; }
        DbSet<Payment> Payments { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        void UpdateRange(params object[] entities);
        void Update<TEntity>(TEntity entity) where TEntity : class;
        DatabaseFacade Database { get; }
    }
    public class TicketingContext : DbContext, ITicketingContext
    {
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<Venue> Venues { get; set; }
        public virtual DbSet<Section> Sections { get; set; }
        public virtual DbSet<Seat> Seats { get; set; }
        public virtual DbSet<PriceOption> PriceOptions { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CartItem> CartItems { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public new DatabaseFacade Database => base.Database;

        public TicketingContext(DbContextOptions<TicketingContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => base.Entry(entity);
        public void UpdateRange(params object[] entities) => base.UpdateRange(entities);
        public void Update<TEntity>(TEntity entity) where TEntity : class => base.Update(entity);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Tickets)
                .WithOne(t => t.Event)
                .HasForeignKey(t => t.EventId);

            modelBuilder.Entity<Venue>()
                .HasMany(v => v.Sections)
                .WithOne(s => s.Venue)
                .HasForeignKey(s => s.VenueId);
            modelBuilder.Entity<CartItem>().HasKey(ci => ci.CartItemId);
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(c => c.CartId);
            modelBuilder.Entity<Seat>()
                .Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (SeatStatus)Enum.Parse(typeof(SeatStatus), v));
        }
    }
}
