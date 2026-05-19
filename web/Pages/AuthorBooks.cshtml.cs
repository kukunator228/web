using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web;

namespace web.Pages
{
    public class AuthorBooksModel : PageModel
    {
        private readonly AppDbContext _context;

        public AuthorBooksModel(AppDbContext context)
        {
            _context = context;
        }

        public Author AuthorInfo { get; set; } = null!;
        public IList<BookAuthor> AuthorBookConnections { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task<IActionResult> OnGetAsync(int authorId)
        {
            if (_context.Authors == null || _context.BookAuthors == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FirstOrDefaultAsync(a => a.AuthorID == authorId);
            if (author == null)
            {
                return NotFound();
            }
            AuthorInfo = author;

            var connectionsQuery = _context.BookAuthors
                .Include(ba => ba.Book)
                .Where(ba => ba.AuthorID == authorId);

            if (!string.IsNullOrEmpty(SearchString))
            {
                string search = SearchString.Trim();
                connectionsQuery = connectionsQuery.Where(ba => EF.Functions.Like(ba.Book.BookName, $"%{search}%"));
            }

            AuthorBookConnections = await connectionsQuery.ToListAsync();

            return Page();
        }
    }
}