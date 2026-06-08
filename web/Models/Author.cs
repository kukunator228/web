using System;
using System.Collections.Generic;

namespace web.Models;

public partial class Author
{
    public int AuthorId { get; set; }

    public string AuthorFirstName { get; set; } = null!;

    public string AuthorSecondName { get; set; } = null!;

    public string? AuthorPatronymic { get; set; }

    public string? AuthorBio { get; set; }

    public string? ImagePath { get; set; }

    public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}
