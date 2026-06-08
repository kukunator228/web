using System;
using System.Collections.Generic;

namespace web.Models;

public partial class BookGenre
{
    public int BookGenreId { get; set; }

    public int BookKey { get; set; }

    public int GenreKey { get; set; }

    public virtual Book BookKeyNavigation { get; set; } = null!;

    public virtual Genre GenreKeyNavigation { get; set; } = null!;
}
