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

        // Поле для выбранного индекса из выпадающего списка
        [BindProperty]
        public string AddressIndex { get; set; } = string.Empty;

        // Текстовый список для хранения всех индексов из таблицы OrderIndex
        public List<string> OrderIndicesList { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            // ИСПРАВЛЕНО: Используем твое точное имя свойства OrderIndices из AppDbContext
            if (_context.OrderIndices != null)
            {
                OrderIndicesList = await _context.OrderIndices
                    .Select(oi => oi.AdressIndex.Trim()) // Используем твое свойство AdressIndex и убираем пробелы
                    .ToListAsync();
            }

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

        // РЕАЛЬНОЕ ОФОРМЛЕНИЕ ЗАКАЗА В БАЗУ ДАННЫХ
        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            // 1. Извлекаем ID залогиненного пользователя из куки (Claims)
            var userIdClaim = User.FindFirst("UserID")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                OrderMessage = "[ОШИБКА]: Для оформления заказа необходимо войти в систему!";
                await OnGetAsync();
                return Page();
            }

            // Проверяем, выставил ли пользователь индекс в выпадающем списке
            if (string.IsNullOrEmpty(AddressIndex))
            {
                OrderMessage = "[ОШИБКА]: Пожалуйста, выберите индекс адреса доставки из списка!";
                await OnGetAsync();
                return Page();
            }

            // Читаем сессию корзины
            var cartSession = HttpContext.Session.GetString("UserCart");
            if (string.IsNullOrEmpty(cartSession))
            {
                OrderMessage = "[ОШИБКА]: Ваша корзина пуста!";
                await OnGetAsync();
                return Page();
            }

            List<int> bookIds = JsonSerializer.Deserialize<List<int>>(cartSession) ?? new List<int>();
            if (!bookIds.Any())
            {
                OrderMessage = "[ОШИБКА]: Ваша корзина пуста!";
                await OnGetAsync();
                return Page();
            }

            // Открываем безопасную транзакцию в MS SQL Server
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Создаем шапку заказа в таблице Order
                var newOrder = new Order
                {
                    AdressIndexKey = AddressIndex.PadRight(10), // Передаем выбранный индекс и дополняем до char(10)
                    OrderDate = DateTime.Now,
                    ClientKey = currentUserId
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync(); // База генерирует уникальный OrderID

                // 3. Группируем товары из сессии для подсчета количества
                var groupedItems = bookIds
                    .GroupBy(id => id)
                    .Select(g => new { BookId = g.Key, Quantity = g.Count() });

                // 4. Заполняем спецификацию заказа в таблице OrderItem
                foreach (var item in groupedItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderKey = newOrder.OrderID, // Внешний ключ на заказ (int)
                        BookKey = item.BookId,       // Внешний ключ на книгу (int)
                        OrderBookQuantity = item.Quantity.ToString().PadRight(5) // С учетом типа char(5) в вашей схеме
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Жестко сохраняем всё в SSMS!

                // 5. Чистим корзину в сессии после успешной покупки
                HttpContext.Session.Remove("UserCart");

                // Перерисовываем пустую корзину на экране
                CartBooks.Clear();
                TotalSum = 0;
                OrderMessage = $"[УСПЕХ]: Заказ №{newOrder.OrderID} успешно оформлен! Все данные внесены в БД.";
                return Page();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Отменяем изменения в базе при ошибке

                // Вытаскиваем самую глубокую причину ошибки из недр SQL Server
                var realErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                OrderMessage = "[КРИТИЧЕСКАЯ ОШИБКА БД]: " + realErrorMessage;

                await OnGetAsync(); // Перезагружаем корзину и выпадающий список
                return Page();
            }
        }
    }
}
