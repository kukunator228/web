using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace web.Pages
{
    public class BooksModel : PageModel
    {
        private readonly AppDbContext _context;

        public BooksModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Book> Books { get; set; } = default!;
        public IList<BookAuthor> BookAuthorsConnections { get; set; } = default!;
        public IList<BookType> BookTypes { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedTypeId { get; set; }

        public async Task OnGetAsync()
        {
            if (_context.BookTypes != null)
            {
                BookTypes = await _context.BookTypes.ToListAsync();
            }

            if (_context.Books != null)
            {
                var booksQuery = _context.Books
                    .Include(b => b.BookType)
                    .Include(b => b.BookReviews)
                    .AsQueryable();

                if (SelectedTypeId.HasValue && SelectedTypeId.Value > 0)
                {
                    booksQuery = booksQuery.Where(b => b.BookTypeID == SelectedTypeId.Value);
                }

                if (!string.IsNullOrEmpty(SearchString))
                {
                    string search = SearchString.Trim();
                    booksQuery = booksQuery.Where(b => EF.Functions.Like(b.BookName, $"%{search}%"));
                }

                Books = await booksQuery.ToListAsync();
            }

            if (_context.BookAuthors != null)
            {
                BookAuthorsConnections = await _context.BookAuthors
                    .Include(ba => ba.Author)
                    .ToListAsync();
            }
        }
    }
}