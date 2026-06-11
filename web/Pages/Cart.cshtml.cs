using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using web.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace web.Pages
{
    public class CartModel : PageModel
    {
        private readonly ElochniBookStore2Context _context;

        public CartModel(ElochniBookStore2Context context)
        {
            _context = context;
        }

        // Модель элемента корзины
        public class CartItemGroup
        {
            public string CartItemId { get; set; } = Guid.NewGuid().ToString();
            public int BookSupplyId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public DateOnly? SupplyDate { get; set; }
            public decimal PricePerUnit { get; set; }
            public int Quantity { get; set; }
            public int MaxAvailable { get; set; }
            public int BookId { get; set; }
            public string BookName { get; set; } = string.Empty;
            public string? BookImagePath { get; set; }
            public int SelectedBookTypeId { get; set; }
            public string SelectedBookTypeName { get; set; } = string.Empty;
            public decimal TotalPrice => PricePerUnit * Quantity;
        }

        public List<CartItemGroup> CartItems { get; set; } = new List<CartItemGroup>();
        public decimal TotalSum { get; set; }
        public string OrderMessage { get; set; } = string.Empty;
        public string MaskedEmail { get; set; } = string.Empty;

        [BindProperty]
        public string AddressIndex { get; set; } = string.Empty;

        public List<string> OrderIndicesList { get; set; } = new List<string>();

        // Настройка сериализации для избежания циклов
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };

        public async Task OnGetAsync()
        {
            if (_context.OrderIndices != null)
            {
                OrderIndicesList = await _context.OrderIndices
                    .Select(oi => oi.Index.Trim())
                    .ToListAsync();
            }
            // Загружаем маскированную почту пользователя
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int currentUserId))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (user != null && !string.IsNullOrEmpty(user.UserEmail)) // Используем Email вместо UserLogIn
                {
                    var email = user.UserEmail;
                    var atIndex = email.IndexOf('@');
                    if (atIndex > 3)
                    {
                        var prefix = email.Substring(0, 3);
                        var domain = email.Substring(atIndex);
                        MaskedEmail = $"{prefix}****{domain}";
                    }
                    else if (atIndex > 0)
                    {
                        var prefix = email.Substring(0, 1);
                        var domain = email.Substring(atIndex);
                        MaskedEmail = $"{prefix}***{domain}";
                    }
                    else
                    {
                        MaskedEmail = email;
                    }
                }
            }

            var cartSession = HttpContext.Session.GetString("UserCart");
            if (string.IsNullOrEmpty(cartSession)) return;

            var cartData = JsonSerializer.Deserialize<List<CartItemGroup>>(cartSession, _jsonOptions);
            if (cartData != null && cartData.Any())
            {
                foreach (var item in cartData)
                {
                    // Физическая книга (BookSupplyId > 0)
                    if (item.BookSupplyId > 0)
                    {
                        var supply = await _context.BookSupplies
                            .Include(bs => bs.SupplierKeyNavigation)
                            .Include(bs => bs.BookKeyNavigation)
                            .FirstOrDefaultAsync(bs => bs.BookSupplyId == item.BookSupplyId);

                        if (supply != null)
                        {
                            item.PricePerUnit = supply.BookSupplyPiecePrice * 1.6m;
                            item.MaxAvailable = int.TryParse(supply.SupplyQuantity, out int q) ? q : 0;
                            item.SupplierName = supply.SupplierKeyNavigation?.SupplierName ?? "Неизвестный";
                            item.SupplyDate = supply.SupplyDate;

                            if (supply.BookKeyNavigation != null)
                            {
                                item.BookId = supply.BookKeyNavigation.BookId;
                                item.BookName = supply.BookKeyNavigation.BookName;
                                item.BookImagePath = supply.BookKeyNavigation.ImagePath;
                            }
                        }
                    }
                    else // Электронная книга (BookSupplyId < 0)
                    {
                        var book = await _context.Books
                            .FirstOrDefaultAsync(b => b.BookId == item.BookId);

                        if (book != null)
                        {
                            item.BookName = book.BookName;
                            item.BookImagePath = book.ImagePath;
                            item.PricePerUnit = 399;
                            item.MaxAvailable = 999;
                            item.SupplierName = "Электронная версия";
                            item.SupplyDate = DateOnly.FromDateTime(DateTime.Now);
                        }
                    }
                }

                CartItems = cartData;
                TotalSum = CartItems.Sum(i => i.TotalPrice);

                HttpContext.Session.SetString("UserCart", JsonSerializer.Serialize(CartItems, _jsonOptions));
            }
        }

        // ОБНОВЛЕНИЕ КОЛИЧЕСТВА (возвращает JSON)
        public IActionResult OnPostUpdateQuantity(string cartItemId, int quantity)
        {
            var cartSession = HttpContext.Session.GetString("UserCart");
            if (!string.IsNullOrEmpty(cartSession))
            {
                var cartData = JsonSerializer.Deserialize<List<CartItemGroup>>(cartSession, _jsonOptions);
                var item = cartData?.FirstOrDefault(i => i.CartItemId == cartItemId);
                if (item != null && quantity > 0 && quantity <= item.MaxAvailable)
                {
                    item.Quantity = quantity;
                    HttpContext.Session.SetString("UserCart", JsonSerializer.Serialize(cartData, _jsonOptions));

                    var totalSum = cartData.Sum(i => i.TotalPrice);
                    return new JsonResult(new { success = true, totalSum = totalSum });
                }
            }
            return new JsonResult(new { success = false });
        }

        // УДАЛЕНИЕ ТОВАРА (возвращает RedirectToPage)
        public IActionResult OnPostRemoveItem(string cartItemId)
        {
            var cartSession = HttpContext.Session.GetString("UserCart");
            if (!string.IsNullOrEmpty(cartSession))
            {
                var cartData = JsonSerializer.Deserialize<List<CartItemGroup>>(cartSession, _jsonOptions);
                var item = cartData?.FirstOrDefault(i => i.CartItemId == cartItemId);
                if (item != null)
                {
                    cartData.Remove(item);
                    HttpContext.Session.SetString("UserCart", JsonSerializer.Serialize(cartData, _jsonOptions));
                }
            }
            return RedirectToPage();
        }

        public IActionResult OnPostClear()
        {
            HttpContext.Session.Remove("UserCart");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                OrderMessage = "Для оформления заказа необходимо войти в систему!";
                await OnGetAsync();
                return Page();
            }

            var hasPhysicalBooks = CartItems.Any(i => i.BookSupplyId > 0);

            // Проверяем индекс ТОЛЬКО если есть физические книги
            if (hasPhysicalBooks && string.IsNullOrEmpty(AddressIndex))
            {
                OrderMessage = "Для доставки физических изданий необходимо выбрать почтовый индекс!";
                await OnGetAsync();
                return Page();
            }

            var cartSession = HttpContext.Session.GetString("UserCart");
            if (string.IsNullOrEmpty(cartSession))
            {
                OrderMessage = "Ваша корзина пуста!";
                await OnGetAsync();
                return Page();
            }

            var cartItems = JsonSerializer.Deserialize<List<CartItemGroup>>(cartSession, _jsonOptions);
            if (cartItems == null || !cartItems.Any())
            {
                OrderMessage = "Ваша корзина пуста!";
                await OnGetAsync();
                return Page();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Для электронных книг используем "0", для физических - выбранный индекс
                string addressIndexValue = hasPhysicalBooks ? AddressIndex.Trim() : "0";

                var newOrder = new Order
                {
                    AdressIndexKey = addressIndexValue,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    ClientKey = currentUserId,
                    StatusKey = 1
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    if (item.BookSupplyId > 0) // Только для физических книг
                    {
                        var supply = await _context.BookSupplies
                            .FirstOrDefaultAsync(bs => bs.BookSupplyId == item.BookSupplyId);

                        if (supply == null)
                        {
                            throw new Exception($"Поставка #{item.BookSupplyId} не найдена");
                        }

                        int availableInSupply = int.TryParse(supply.SupplyQuantity, out int q) ? q : 0;
                        if (item.Quantity > availableInSupply)
                        {
                            throw new Exception($"Для издания \"{item.BookName}\" доступно только {availableInSupply} шт.");
                        }

                        supply.SupplyQuantity = (availableInSupply - item.Quantity).ToString();
                        _context.BookSupplies.Update(supply);
                    }

                    var orderItem = new OrderItem
                    {
                        OrderKey = newOrder.OrderId,
                        BookKey = item.BookId,
                        OrderBookQuantity = item.Quantity.ToString()
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                HttpContext.Session.Remove("UserCart");

                CartItems.Clear();
                TotalSum = 0;

                TempData["SuccessMessage"] = $"Спасибо за покупку! Ваш заказ №{newOrder.OrderId} успешно оформлен.";

                return Page();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var realErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                OrderMessage = "Ошибка при оформлении заказа: " + realErrorMessage;
                await OnGetAsync();
                return Page();
            }
        }
    }
}