using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Book> PopularBooks { get; set; } = new List<Book>();
        public List<BookReview> PopularReviews { get; set; } = new List<BookReview>();

        public async Task OnGetAsync()
        {
            if (_context.Books != null)
            {
                var allBooks = await _context.Books
                    .Include(b => b.BookType)
                    .Include(b => b.BookReviews)
                    .ToListAsync();

                PopularBooks = allBooks
                    .Where(b => b.BookReviews != null && b.BookReviews.Any())
                    .OrderByDescending(b => b.BookReviews.Average(r => r.BookScore))
                    .ThenByDescending(b => b.BookReviews.Count)
                    .Take(5)
                    .ToList();

                if (PopularBooks.Count < 5)
                {
                    var extraBooks = allBooks
                        .Where(b => !PopularBooks.Contains(b))
                        .Take(5 - PopularBooks.Count);
                    PopularBooks.AddRange(extraBooks);
                }
            }

            if (_context.BookReviews != null)
            {
                PopularReviews = await _context.BookReviews
                    .Include(r => r.User)
                    .Include(r => r.Book)
                    .Where(r => r.RatingSum > 0)
                    .OrderByDescending(r => r.RatingSum)
                    .Take(3)
                    .ToListAsync();
            }
        }
    }
}