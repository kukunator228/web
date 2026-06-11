using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace web.Models;

public partial class ElochniBookStore2Context : DbContext
{
    public ElochniBookStore2Context()
    {
    }

    public ElochniBookStore2Context(DbContextOptions<ElochniBookStore2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<AdressIndex> AdressIndices { get; set; }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookAuthor> BookAuthors { get; set; }

    public virtual DbSet<BookBooktype> BookBooktypes { get; set; }

    public virtual DbSet<BookGenre> BookGenres { get; set; }

    public virtual DbSet<BookReview> BookReviews { get; set; }

    public virtual DbSet<BookSupply> BookSupplies { get; set; }

    public virtual DbSet<BookType> BookTypes { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderIndex> OrderIndices { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdressIndex>(entity =>
        {
            entity.HasKey(e => e.AdressId);

            entity.ToTable("AdressIndex");

            entity.Property(e => e.AdressId)
                .ValueGeneratedNever()
                .HasColumnName("AdressID");
            entity.Property(e => e.AdressName).HasMaxLength(255);
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("Author");

            entity.Property(e => e.AuthorId).HasColumnName("AuthorId");
            entity.Property(e => e.AuthorFirstName).HasMaxLength(50);
            entity.Property(e => e.AuthorPatronymic).HasMaxLength(50);
            entity.Property(e => e.AuthorSecondName).HasMaxLength(50);
            entity.Property(e => e.ImagePath).HasMaxLength(255);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK_Book_1");

            entity.ToTable("Book");

            entity.Property(e => e.BookId).HasColumnName("BookId");
            entity.Property(e => e.BookDesc).HasMaxLength(300);
            entity.Property(e => e.BookName).HasMaxLength(50);
            entity.Property(e => e.ImagePath).HasMaxLength(255);
        });

        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity.Property(e => e.BookAuthorId).HasColumnName("BookAuthorId");
            entity.Property(e => e.AuthorId).HasColumnName("AuthorId");
            entity.Property(e => e.BookId).HasColumnName("BookId");

            entity.HasOne(d => d.Author).WithMany(p => p.BookAuthors)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookAuthors_Author");

            entity.HasOne(d => d.Book).WithMany(p => p.BookAuthors)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookAuthors_Book");
        });

        modelBuilder.Entity<BookBooktype>(entity =>
        {
            entity.HasKey(e => e.BookBooktypeId).HasName("PK_BookBooktype_1");

            entity.ToTable("BookBooktype");

            entity.Property(e => e.BookBooktypeId).HasColumnName("BookBooktypeID");

            entity.HasOne(d => d.BookKeyNavigation).WithMany(p => p.BookBooktypes)
                .HasForeignKey(d => d.BookKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookBooktype_Book");

            entity.HasOne(d => d.BookTypeKeyNavigation).WithMany(p => p.BookBooktypes)
                .HasForeignKey(d => d.BookTypeKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookBooktype_BookType");
        });

        modelBuilder.Entity<BookGenre>(entity =>
        {
            entity.ToTable("BookGenre");

            entity.Property(e => e.BookGenreId).HasColumnName("BookGenreID");

            entity.HasOne(d => d.BookKeyNavigation).WithMany(p => p.BookGenres)
                .HasForeignKey(d => d.BookKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookGenre_Book");

            entity.HasOne(d => d.GenreKeyNavigation).WithMany(p => p.BookGenres)
                .HasForeignKey(d => d.GenreKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookGenre_Genre");
        });

        modelBuilder.Entity<BookReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__BookRevi__74BC79AE0FE3A43E");

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.BookScore).HasDefaultValue(5);
            entity.Property(e => e.ReviewDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.BookKeyNavigation).WithMany(p => p.BookReviews)
                .HasForeignKey(d => d.BookKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Books");

            entity.HasOne(d => d.UserKeyNavigation).WithMany(p => p.BookReviews)
                .HasForeignKey(d => d.UserKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Users");
        });

        modelBuilder.Entity<BookSupply>(entity =>
        {
            entity.ToTable("BookSupply");

            entity.HasKey(e => e.BookSupplyId);

            entity.Property(e => e.BookSupplyId).HasColumnName("BookSupplyID");
            entity.Property(e => e.BookKey).HasColumnName("BookKey");
            entity.Property(e => e.SupplierKey).HasColumnName("SupplierKey");
            entity.Property(e => e.SupplyQuantity).HasColumnName("SupplyQuantity");
            entity.Property(e => e.SupplyDate).HasColumnName("SupplyDate");
            entity.Property(e => e.BookSupplyPiecePrice).HasColumnName("BookSupplyPiecePrice");

            entity.HasOne(bs => bs.BookKeyNavigation)
                .WithMany(b => b.BookSupplies)
                .HasForeignKey(bs => bs.BookKey)
                .HasPrincipalKey(b => b.BookId);

            entity.HasOne(bs => bs.SupplierKeyNavigation)
                .WithMany(s => s.BookSupplies)
                .HasForeignKey(bs => bs.SupplierKey)
                .HasPrincipalKey(s => s.SupplierId);
        });

        modelBuilder.Entity<BookType>(entity =>
        {
            entity.ToTable("BookType");

            entity.Property(e => e.BookTypeId).HasColumnName("BookTypeID");
            entity.Property(e => e.BookTypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Client");

            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ClientEmail).HasMaxLength(30);
            entity.Property(e => e.ClientFinstName).HasMaxLength(50);
            entity.Property(e => e.ClientPatronymic).HasMaxLength(50);
            entity.Property(e => e.ClientSecondName).HasMaxLength(50);
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.ToTable("Genre");

            entity.Property(e => e.GenreId).HasColumnName("GenreID");
            entity.Property(e => e.GenreName).HasMaxLength(50);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.AdressIndexKey)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StatusKey).HasDefaultValue(1);

            entity.HasOne(d => d.AdressIndexKeyNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AdressIndexKey)
                .HasConstraintName("FK_Order_OrderIndex1");

            entity.HasOne(d => d.ClientKeyNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ClientKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Users");

            entity.HasOne(d => d.StatusKeyNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StatusKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_OrderStatuses");
        });

        modelBuilder.Entity<OrderIndex>(entity =>
        {
            entity.HasKey(e => e.Index);

            entity.ToTable("OrderIndex");

            entity.Property(e => e.Index)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItem");

            entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
            entity.Property(e => e.OrderBookQuantity)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.BookKeyNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.BookKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItem_Book");

            entity.HasOne(d => d.OrderKeyNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderKey)
                .HasConstraintName("FK_OrderItem_Order");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__OrderSta__C8EE20434770D4D8");

            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Supplier");

            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.SupplierInn)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SupplierINN");
            entity.Property(e => e.SupplierName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_OrderType");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.UserLogIn).HasMaxLength(50);
            entity.Property(e => e.UserEmail).HasMaxLength(50);
            entity.Property(e => e.UserPassword).HasMaxLength(50);

            entity.HasOne(d => d.RoleKeyNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleKey)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}