using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Models;

namespace web.Pages
{
    public class MyOrdersModel : PageModel
    {
        private readonly ElochniBookStore2Context _context;

        public MyOrdersModel(ElochniBookStore2Context context)
        {
            _context = context;
        }

        public List<Order> UserOrders { get; set; } = new List<Order>();
        public List<BookReview> UserReviews { get; set; } = new List<BookReview>();
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                ErrorMessage = "Для просмотра истории заказов необходимо войти в систему!";
                return Page();
            }

            if (_context.Orders != null)
            {
                UserOrders = await _context.Orders
                    .Where(o => o.ClientKey == currentUserId)
                    .Include(o => o.StatusKeyNavigation)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.BookKeyNavigation)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }

            // Загружаем отзывы пользователя
            UserReviews = await _context.BookReviews
                .Where(r => r.UserKey == currentUserId)
                .ToListAsync();

            return Page();
        }
    }
}