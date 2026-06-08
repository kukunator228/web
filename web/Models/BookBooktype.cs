using System;
using System.Collections.Generic;

namespace web.Models;

public partial class BookBooktype
{
    public int BookBooktypeId { get; set; }

    public int BookKey { get; set; }

    public int BookTypeKey { get; set; }

    public virtual Book BookKeyNavigation { get; set; } = null!;

    public virtual BookType BookTypeKeyNavigation { get; set; } = null!;
}
