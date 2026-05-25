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
            });

            modelBuilder.Entity<BookAuthor>(entity =>
            {
                entity.ToTable("BookAuthors");
                entity.HasKey(e => e.BookAuthorID);
                entity.Property(e => e.BookAuthorID).HasColumnName("BookAuthorID");
                entity.Property(e => e.BookID).HasColumnName("BookID");
                entity.Property(e => e.AuthorID).HasColumnName("AuthorID");
            });
        }
    }
}