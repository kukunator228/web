using System;
using System.Collections.Generic;

namespace web.Models;

public partial class BookType
{
    public int BookTypeId { get; set; }

    public string BookTypeName { get; set; } = null!;

    public virtual ICollection<BookBooktype> BookBooktypes { get; set; } = new List<BookBooktype>();
}
