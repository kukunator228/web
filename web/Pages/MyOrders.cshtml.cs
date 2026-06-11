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
        public string MaskedEmail { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                ErrorMessage = "Для просмотра истории заказов необходимо войти в систему!";
                return Page();
            }

            // Получаем email пользователя и маскируем его
            var user = await _context.Users
                .Where(u => u.UserId == currentUserId)
                .Select(u => u.UserEmail)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(user))
            {
                MaskedEmail = MaskEmail(user);
            }

            if (_context.Orders != null)
            {
                UserOrders = await _context.Orders
                    .Where(o => o.ClientKey == currentUserId)
                    .Include(o => o.StatusKeyNavigation)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.BookKeyNavigation)
                    .OrderByDescending(o => o.OrderDate)
                    .AsNoTracking()
                    .ToListAsync();
            }

            // Загружаем отзывы пользователя
            UserReviews = await _context.BookReviews
                .Where(r => r.UserKey == currentUserId)
                .AsNoTracking()
                .ToListAsync();

            return Page();
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;

            var atIndex = email.IndexOf('@');

            if (atIndex > 0)
            {
                // Маскируем часть до @
                if (atIndex <= 2)
                {
                    // Очень короткий логин, например "a@mail.ru" -> "a***@mail.ru"
                    return email[0] + "***" + email.Substring(atIndex);
                }
                else
                {
                    // Стандартная маскировка: первые 2 символа + *** + домен
                    return email.Substring(0, 2) + "***" + email.Substring(atIndex);
                }
            }
            else
            {
                // Если нет @, просто маскируем часть
                if (email.Length <= 2)
                    return email[0] + "***";
                else
                    return email.Substring(0, 2) + "***";
            }
        }
    }
}