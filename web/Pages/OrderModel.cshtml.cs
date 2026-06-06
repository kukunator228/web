using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace web.Pages
{
    public class OrderModel : PageModel
    {
        private readonly AppDbContext _context;

        public OrderModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string AddressIndex { get; set; } = string.Empty;

        public List<Book> CartBooks { get; set; } = new List<Book>();

        // Метод отображения товаров в корзине перед оформлением заказа
        public async Task<IActionResult> OnGetAsync()
        {
            var cartSession = HttpContext.Session.GetString("UserCart");
            if (string.IsNullOrEmpty(cartSession)) return Page();

            var bookIds = JsonSerializer.Deserialize<List<int>>(cartSession);
            if (bookIds != null && bookIds.Any())
            {
                CartBooks = await _context.Books
                    .Where(b => bookIds.Contains(b.BookID))
                    .ToListAsync();
            }

            return Page();
        }

        // Обработчик кнопки "Оформить заказ"
        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            // 1. Извлекаем ID текущего залогиненного пользователя из твоих Claims (куки)
            var userIdClaim = User.FindFirst("UserID")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                ModelState.AddModelError(string.Empty, "Для оформления заказа нужно сначала войти в аккаунт!");
                await OnGetAsync();
                return Page();
            }

            // Читаем список добавленных книг из сессии корзины
            var cartSession = HttpContext.Session.GetString("UserCart");
            if (string.IsNullOrEmpty(cartSession)) return RedirectToPage("/Books");

            var bookIds = JsonSerializer.Deserialize<List<int>>(cartSession);
            if (bookIds == null || !bookIds.Any()) return RedirectToPage("/Books");

            // Запускаем безопасную транзакцию в базе данных SSMS
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Создаем главную запись заказа в таблице Order (согласно твоей схеме)
                var newOrder = new Order
                {
                    AdressIndexKey = AddressIndex, // Поле char(10) из схемы
                    OrderDate = DateTime.Now,      // Поле date из схемы
                    ClientKey = currentUserId      // Поле int из схемы (ID нашего покупателя)
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync(); // База данных сгенерирует уникальный OrderID

                // 3. Группируем ID книг из корзины, чтобы посчитать их количество
                var groupedItems = bookIds
                    .GroupBy(id => id)
                    .Select(g => new { BookId = g.Key, Quantity = g.Count() });

                // 4. Заполняем позициями таблицу OrderItem для каждой уникальной книги
                foreach (var item in groupedItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderKey = newOrder.OrderID, // Привязка к созданной шапке заказа (int)
                        BookKey = item.BookId,       // Привязка к книге (int)
                        OrderBookQuantity = item.Quantity.ToString().PadRight(5) // В схеме тип char(5), дополняем пробелами
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Подтверждаем транзакцию, данные сохраняются в БД

                // 5. Полностью очищаем корзину в сессии после успешной покупки
                HttpContext.Session.Remove("UserCart");

                // Перенаправляем на страницу успешного оформления (создай пустую страницу OrderSuccess)
                return RedirectToPage("/OrderSuccess");
            }
            catch (Exception ex)
            {
                // Если произошел сбой — отменяем все изменения в базе данных
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Произошла ошибка при сохранении заказа: " + ex.Message);
                await OnGetAsync();
                return Page();
            }
        }
    }
}
