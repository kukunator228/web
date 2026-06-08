using System;
using System.Collections.Generic;

namespace web.Models;

public partial class BookReview
{
    public int ReviewId { get; set; }

    public int BookKey { get; set; }

    public int UserKey { get; set; }

    public string ReviewText { get; set; } = null!;

    public DateTime? ReviewDate { get; set; }

    public int BookScore { get; set; }

    public virtual Book BookKeyNavigation { get; set; } = null!;

    public virtual User UserKeyNavigation { get; set; } = null!;
}
