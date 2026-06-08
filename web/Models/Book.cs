using System;
using System.Collections.Generic;

namespace web.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string BookName { get; set; } = null!;

    public string BookDesc { get; set; } = null!;

    public string? ImagePath { get; set; }

    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

    public virtual ICollection<BookBooktype> BookBooktypes { get; set; } = new List<BookBooktype>();

    public virtual ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

    public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<BookSupply> BookSupplies { get; set; } = new List<BookSupply>();
}
