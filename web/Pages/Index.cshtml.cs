using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web;

namespace web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public int TotalBooksCount { get; set; }
        public int TotalAudioCount { get; set; }
        public int TotalAuthorsCount { get; set; }

        public IList<Book> RecentBooks { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Books != null)
            {
                TotalBooksCount = await _context.Books.CountAsync(b => b.BookTypeID == 1);
                TotalAudioCount = await _context.Books.CountAsync(b => b.BookTypeID == 2);

                RecentBooks = await _context.Books
                    .OrderByDescending(b => b.BookID)
                    .Take(5)
                    .ToListAsync();
            }

            if (_context.Authors != null)
            {
                TotalAuthorsCount = await _context.Authors.CountAsync();
            }
        }
    }
}