using Microsoft.EntityFrameworkCore;

namespace web
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<BookType> BookTypes { get; set; } = null!;
        public DbSet<OrderIndex> OrderIndices { get; set; } = null!;

        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<BookSupply> BookSupplies { get; set; } = null!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<BookAuthor> BookAuthors { get; set; } = null!;
        public DbSet<BookReview> BookReviews { get; set; } = null!;
        public DbSet<ReviewVote> ReviewVotes { get; set; } = null!;
        public DbSet<OrderStatus> OrderStatuses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("Client");
                entity.Property(e => e.ClientID).HasColumnName("ClientID");
                entity.Property(e => e.ClientFirstName).HasColumnName("ClientFirstName");
                entity.Property(e => e.ClientSecondName).HasColumnName("ClientSecondName");
                entity.Property(e => e.ClientPatronymic).HasColumnName("ClientPatronymic");
                entity.Property(e => e.ClientEmail).HasColumnName("ClientEmail");

                entity.HasMany(c => c.Orders)
                      .WithOne()
                      .HasForeignKey(o => o.ClientKey);
            });

            modelBuilder.Entity<BookAuthor>(entity =>
            {
                entity.ToTable("BookAuthors");
                entity.HasKey(e => e.BookAuthorID);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");
                entity.HasKey(e => e.OrderID);

                entity.HasMany(o => o.OrderItems)
                      .WithOne()
                      .HasForeignKey(oi => oi.OrderKey);

                entity.HasOne(o => o.OrderStatus)
                      .WithMany(s => s.Orders)
                      .HasForeignKey(o => o.StatusKey);
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.ToTable("OrderStatuses");
                entity.HasKey(e => e.StatusID);
            });
        }
    }
}