using System;
using System.Collections.Generic;

namespace web.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserLogIn { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public string UserEmail { get; set; }

    public int RoleKey { get; set; }

    public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

}