using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using web.Models;

namespace web.Pages
{
    public class BookDetailsModel : PageModel
    {
        private readonly ElochniBookStore2Context _context;

        public BookDetailsModel(ElochniBookStore2Context context)
        {
            _context = context;
        }

        public Book? CurrentBook { get; set; }
        public List<Author> AuthorsList { get; set; } = new List<Author>();
        public bool IsAddedSuccess { get; set; } = false;
        public List<BookReview> ReviewsList { get; set; } = new List<BookReview>();
        public double AverageScore { get; set; } = 0.0;
        public string ReviewErrorMessage { get; set; } = string.Empty;
        public bool CanLeaveReview { get; set; } = false;
        public bool UserHasReview { get; set; } = false;

        [BindProperty]
        public string NewReviewText { get; set; } = string.Empty;

        [BindProperty]
        public int NewReviewScore { get; set; } = 5;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return RedirectToPage("/Books");
            }

            CurrentBook = await _context.Books
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (CurrentBook == null)
            {
                ReviewErrorMessage = $"Книга с ID={id} не найдена в базе данных";
                return Page();
            }

            CurrentBook.BookBooktypes = await _context.BookBooktypes
                .Where(bb => bb.BookKey == id)
                .Include(bb => bb.BookTypeKeyNavigation)
                .ToListAsync();

            CurrentBook.BookSupplies = await _context.BookSupplies
                .Where(bs => bs.BookKey == id)
                .Include(bs => bs.SupplierKeyNavigation)
                .ToListAsync();

            AuthorsList = await _context.BookAuthors
                .Where(ba => ba.BookId == CurrentBook.BookId)
                .Include(ba => ba.Author)
                .Select(ba => ba.Author)
                .Where(a => a != null)
                .ToListAsync();

            ReviewsList = await _context.BookReviews
                .Where(r => r.BookKey == CurrentBook.BookId)
                .Include(r => r.UserKeyNavigation)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            if (ReviewsList.Any())
            {
                AverageScore = Math.Round(ReviewsList.Average(r => r.BookScore), 1);
            }

            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int currentUserId))
            {
                var hasOrderWithStatus4 = await _context.OrderItems
                    .Include(oi => oi.OrderKeyNavigation)
                    .AnyAsync(oi => oi.BookKey == id &&
                                   oi.OrderKeyNavigation.ClientKey == currentUserId &&
                                   oi.OrderKeyNavigation.StatusKey == 4);

                UserHasReview = await _context.BookReviews
                    .AnyAsync(r => r.BookKey == id && r.UserKey == currentUserId);

                CanLeaveReview = hasOrderWithStatus4 && !UserHasReview;
            }

            return Page();
        }

        // Добавление физической книги в корзину
        public async Task<IActionResult> OnPostAddToCartAsync(int supplyId, int quantity, int bookTypeId)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                TempData["CartErrorMessage"] = "Для добавления товаров в корзину необходимо войти в систему!";
                return RedirectToPage("/Login");
            }

            var supply = await _context.BookSupplies
                .AsNoTracking()
                .Include(bs => bs.BookKeyNavigation)
                .FirstOrDefaultAsync(bs => bs.BookSupplyId == supplyId);

            if (supply == null)
            {
                TempData["CartErrorMessage"] = "Товар не найден!";
                return RedirectToPage("/Books");
            }

            int available = int.TryParse(supply.SupplyQuantity, out int q) ? q : 0;
            if (quantity > available)
            {
                TempData["CartErrorMessage"] = $"Доступно только {available} шт.";
                return RedirectToPage(new { id = supply.BookKey });
            }

            var bookType = await _context.BookTypes
                .FirstOrDefaultAsync(bt => bt.BookTypeId == bookTypeId);

            var cartSession = HttpContext.Session.GetString("UserCart");
            List<CartItem> cartItems = string.IsNullOrEmpty(cartSession)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartSession) ?? new List<CartItem>();

            var existingItem = cartItems.FirstOrDefault(i => i.BookSupplyId == supplyId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                if (existingItem.Quantity > existingItem.MaxAvailable)
                {
                    existingItem.Quantity = existingItem.MaxAvailable;
                }
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    CartItemId = Guid.NewGuid().ToString(),
                    BookId = supply.BookKey,
                    BookName = supply.BookKeyNavigation?.BookName ?? "Книга",
                    BookImagePath = supply.BookKeyNavigation?.ImagePath,
                    BookSupplyId = supply.BookSupplyId,
                    SupplierName = supply.SupplierKeyNavigation?.SupplierName ?? "Неизвестный",
                    PricePerUnit = supply.BookSupplyPiecePrice * 1.6m,
                    Quantity = quantity,
                    MaxAvailable = available,
                    SupplyDate = supply.SupplyDate,
                    SelectedBookTypeId = bookTypeId,
                    SelectedBookTypeName = bookType?.BookTypeName ?? "Физическая книга"
                });
            }

            HttpContext.Session.SetString("UserCart", JsonSerializer.Serialize(cartItems));

            IsAddedSuccess = true;

            TempData["CartMessage"] = $"Товар \"{supply.BookKeyNavigation?.BookName}\" добавлен в корзину!";

            return RedirectToPage(new { id = supply.BookKey });
        }

        // Добавление электронной книги в корзину
        public async Task<IActionResult> OnPostAddElectronicToCartAsync(int bookId, int bookTypeId, int quantity)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                TempData["CartErrorMessage"] = "Для добавления товаров в корзину необходимо войти в систему!";
                return RedirectToPage("/Login");
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null)
            {
                TempData["CartErrorMessage"] = "Книга не найдена!";
                return RedirectToPage("/Books");
            }

            var bookType = await _context.BookTypes
                .FirstOrDefaultAsync(bt => bt.BookTypeId == bookTypeId);

            int virtualSupplyId = -bookTypeId;

            var cartSession = HttpContext.Session.GetString("UserCart");
            List<CartItem> cartItems = string.IsNullOrEmpty(cartSession)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartSession) ?? new List<CartItem>();

            decimal electronicPrice = 399;

            var existingItem = cartItems.FirstOrDefault(i => i.BookSupplyId == virtualSupplyId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    CartItemId = Guid.NewGuid().ToString(),
                    BookId = book.BookId,
                    BookName = book.BookName,
                    BookImagePath = book.ImagePath,
                    BookSupplyId = virtualSupplyId,
                    SupplierName = "Электронная версия",
                    PricePerUnit = electronicPrice,
                    Quantity = quantity,
                    MaxAvailable = 999,
                    SupplyDate = DateOnly.FromDateTime(DateTime.Now),
                    SelectedBookTypeId = bookTypeId,
                    SelectedBookTypeName = bookType?.BookTypeName ?? "Электронная книга"
                });
            }

            HttpContext.Session.SetString("UserCart", JsonSerializer.Serialize(cartItems));

            IsAddedSuccess = true;

            TempData["CartMessage"] = $"Электронная книга \"{book.BookName}\" добавлена в корзину!";

            return RedirectToPage(new { id = book.BookId });
        }

        public async Task<IActionResult> OnPostAddReviewAsync(int id)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                await OnGetAsync(id);
                ReviewErrorMessage = "Только зарегистрированные пользователи могут оставлять отзывы!";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewReviewText))
            {
                await OnGetAsync(id);
                ReviewErrorMessage = "Текст отзыва не может быть пустым";
                return Page();
            }

            var hasReview = await _context.BookReviews
                .AnyAsync(r => r.BookKey == id && r.UserKey == currentUserId);

            if (hasReview)
            {
                await OnGetAsync(id);
                ReviewErrorMessage = "Вы уже оставили отзыв на эту книгу!";
                return Page();
            }

            var review = new BookReview
            {
                BookKey = id,
                UserKey = currentUserId,
                ReviewText = NewReviewText.Trim(),
                ReviewDate = DateTime.Now,
                BookScore = NewReviewScore
            };

            _context.BookReviews.Add(review);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                ReviewErrorMessage = $"Ошибка при сохранении отзыва: {innerMessage}";
                await OnGetAsync(id);
                return Page();
            }

            return RedirectToPage(new { id });
        }

        public class CartItem
        {
            public string CartItemId { get; set; } = Guid.NewGuid().ToString();
            public int BookId { get; set; }
            public string BookName { get; set; } = string.Empty;
            public string? BookImagePath { get; set; }
            public int BookSupplyId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public DateOnly? SupplyDate { get; set; }
            public decimal PricePerUnit { get; set; }
            public int Quantity { get; set; }
            public int MaxAvailable { get; set; }
            public int SelectedBookTypeId { get; set; }
            public string SelectedBookTypeName { get; set; } = string.Empty;
            public decimal TotalPrice => PricePerUnit * Quantity;
        }
    }
}