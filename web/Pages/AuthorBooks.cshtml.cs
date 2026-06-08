using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Models;

namespace web.Pages
{
    public class AuthorBooksModel : PageModel
    {
        private readonly ElochniBookStore2Context _context;

        public AuthorBooksModel(ElochniBookStore2Context context)
        {
            _context = context;
        }

        public Author AuthorInfo { get; set; } = null!;
        public IList<BookAuthor> AuthorBookConnections { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task<IActionResult> OnGetAsync(int AuthorId)
        {
            if (_context.Authors == null || _context.BookAuthors == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FirstOrDefaultAsync(a => a.AuthorId == AuthorId);
            if (author == null)
            {
                return NotFound();
            }
            AuthorInfo = author;

            var connectionsQuery = _context.BookAuthors
                .Include(ba => ba.Book)
                .Where(ba => ba.AuthorId == AuthorId);

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