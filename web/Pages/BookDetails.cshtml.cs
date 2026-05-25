using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace web.Pages
{
    public class BookDetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public BookDetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Book? CurrentBook { get; set; }
        public List<Author> AuthorsList { get; set; } = new List<Author>();
        public bool IsAddedSuccess { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Books == null) return Page();

            CurrentBook = await _context.Books
                .Include(b => b.BookType)
                .FirstOrDefaultAsync(b => b.BookID == id);

            if (CurrentBook != null && _context.BookAuthors != null)
            {
                AuthorsList = await _context.BookAuthors
                    .Where(ba => ba.BookID == CurrentBook.BookID)
                    .Select(ba => ba.Author)
                    .Where(a => a != null)
                    .ToListAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Books == null) return Page();

            await OnGetAsync(id);

            var cartSession = HttpContext.Session.GetString("UserCart");
            List<int> cartItems = string.IsNullOrEmpty(cartSession)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(cartSession) ?? new List<int>();

            cartItems.Add(id.Value);

            HttpContext.Session.SetString("UserCart", JsonSerializer.Serialize(cartItems));

            IsAddedSuccess = true;
            return Page();
        }
    }
}