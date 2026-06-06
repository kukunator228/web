using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace web.Pages
{
    public class MyOrdersModel : PageModel
    {
        private readonly AppDbContext _context;

        public MyOrdersModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Order> UserOrders { get; set; } = new List<Order>();
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
                    .Include(o => o.OrderStatus)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Book)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }

            return Page();
        }
    }
}