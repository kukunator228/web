using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace web.Pages
{
    public class CartModel : PageModel
    {
        private readonly AppDbContext _context;

        public CartModel(AppDbContext context)
        {
            _context = context;
        }

        public class CartItemGroup
        {
            public Book Book { get; set; } = null!;
            public int Quantity { get; set; }
        }

        public List<CartItemGroup> CartBooks { get; set; } = new List<CartItemGroup>();
        public decimal TotalSum { get; set; }
        public string OrderMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var cartSession = HttpContext.Session.GetString("UserCart");
            if (string.IsNullOrEmpty(cartSession)) return;

            List<int> bookIds = JsonSerializer.Deserialize<List<int>>(cartSession) ?? new List<int>();

            if (bookIds.Any() && _context.Books != null)
            {

                var books = await _context.Books
                    .Where(b => bookIds.Contains(b.BookID))
                    .ToListAsync();

                CartBooks = bookIds
                    .GroupBy(id => id)
                    .Select(g => new CartItemGroup
                    {
                        Book = books.First(b => b.BookID == g.Key),
                        Quantity = g.Count()
                    })
                    .ToList();

                TotalSum = CartBooks.Sum(item => item.Book.Price * item.Quantity);
            }
        }

        public IActionResult OnPostClear()
        {
            HttpContext.Session.Remove("UserCart");
            return RedirectToPage();
        }

        public IActionResult OnPostCheckout()
        {

            HttpContext.Session.Remove("UserCart");
            OrderMessage = "[СИСТЕМА]: Накладная успешно сформирована. Складской резерв обновлен!";
            return Page();
        }
    }
}