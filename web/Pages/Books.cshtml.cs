using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using web.Models;

namespace web.Pages
{
    public class BooksModel : PageModel
    {
        private readonly ElochniBookStore2Context _context;

        public BooksModel(ElochniBookStore2Context context)
        {
            _context = context;
        }

        public IList<Book> Books { get; set; } = default!;
        public Dictionary<int, List<Author>> BookAuthorsDict { get; set; } = new();
        public IList<BookType> BookTypes { get; set; } = default!;
        public IList<Genre> Genres { get; set; } = default!;
        public IList<Supplier> Suppliers { get; set; } = default!;
        public IList<Author> Authors { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedTypeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedGenreId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedSupplierId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedAuthorId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedAuthorName { get; set; } // ← ДОБАВЬ ЭТУ СТРОКУ

        public async Task OnGetAsync()
        {
            // Парсим выбранного автора из строки (формат "ID|Имя")
            if (!string.IsNullOrEmpty(SelectedAuthorName) && SelectedAuthorName.Contains("|"))
            {
                var parts = SelectedAuthorName.Split('|');
                if (int.TryParse(parts[0], out int authorId))
                {
                    SelectedAuthorId = authorId;
                }
            }

            await LoadBookTypesAsync();
            await LoadGenresAsync();
            await LoadSuppliersAsync();
            await LoadAuthorsAsync();

            await LoadBooksAsync();
            await LoadAuthorsForBooksAsync();
        }

        private async Task LoadBookTypesAsync()
        {
            if (_context.BookTypes != null)
            {
                BookTypes = await _context.BookTypes.AsNoTracking().ToListAsync();
            }
        }

        private async Task LoadGenresAsync()
        {
            if (_context.Genres != null)
            {
                Genres = await _context.Genres.AsNoTracking().ToListAsync();
            }
        }

        private async Task LoadSuppliersAsync()
        {
            if (_context.Suppliers != null)
            {
                Suppliers = await _context.Suppliers.AsNoTracking().ToListAsync();
            }
        }

        private async Task LoadAuthorsAsync()
        {
            if (_context.Authors != null)
            {
                Authors = await _context.Authors.AsNoTracking().ToListAsync();
            }
        }

        private async Task LoadBooksAsync()
        {
            if (_context.Books == null) return;

            var booksQuery = _context.Books
                .AsNoTracking()
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.GenreKeyNavigation)
                .AsQueryable();

            // Фильтрация по типу книги
            if (SelectedTypeId.HasValue && SelectedTypeId.Value > 0)
            {
                booksQuery = booksQuery.Where(b =>
                    _context.BookBooktypes.Any(bb => bb.BookKey == b.BookId && bb.BookTypeKey == SelectedTypeId.Value));
            }

            // Фильтрация по жанру
            if (SelectedGenreId.HasValue && SelectedGenreId.Value > 0)
            {
                booksQuery = booksQuery.Where(b =>
                    _context.BookGenres.Any(bg => bg.BookKey == b.BookId && bg.GenreKey == SelectedGenreId.Value));
            }

            // Фильтрация по поставщику
            if (SelectedSupplierId.HasValue && SelectedSupplierId.Value > 0)
            {
                booksQuery = booksQuery.Where(b =>
                    _context.BookSupplies.Any(bs => bs.BookKey == b.BookId && bs.SupplierKey == SelectedSupplierId.Value));
            }

            // Фильтрация по автору
            if (SelectedAuthorId.HasValue && SelectedAuthorId.Value > 0)
            {
                booksQuery = booksQuery.Where(b =>
                    _context.BookAuthors.Any(ba => ba.BookId == b.BookId && ba.AuthorId == SelectedAuthorId.Value));
            }

            // Поиск по названию
            if (!string.IsNullOrEmpty(SearchString))
            {
                string search = SearchString.Trim();
                booksQuery = booksQuery.Where(b => EF.Functions.Like(b.BookName, $"%{search}%"));
            }

            Books = await booksQuery.ToListAsync();
        }

        private async Task LoadAuthorsForBooksAsync()
        {
            if (!Books.Any() || _context.BookAuthors == null) return;

            var bookIds = Books.Select(b => b.BookId).ToList();

            var authorsData = await _context.BookAuthors
                .AsNoTracking()
                .Where(ba => bookIds.Contains(ba.BookId))
                .Include(ba => ba.Author)
                .Select(ba => new { ba.BookId, Author = ba.Author })
                .Where(x => x.Author != null)
                .ToListAsync();

            BookAuthorsDict = authorsData
                .GroupBy(x => x.BookId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Author!).ToList()
                );
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int bookId)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                TempData["CartMessage"] = "Для добавления в корзину необходимо войти в систему!";
                return RedirectToPage("/Login");
            }

            var book = await _context.Books
                .Include(b => b.BookSupplies)
                    .ThenInclude(bs => bs.SupplierKeyNavigation)
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null)
            {
                TempData["CartMessage"] = "Книга не найдена!";
                return RedirectToPage();
            }

            var cheapestSupply = book.BookSupplies?
                .Where(s => int.TryParse(s.SupplyQuantity, out int q) && q > 0)
                .OrderBy(s => s.BookSupplyPiecePrice * 1.6m)
                .FirstOrDefault();

            if (cheapestSupply == null)
            {
                TempData["CartMessage"] = "Нет доступных предложений для этой книги!";
                return RedirectToPage();
            }

            int available = int.TryParse(cheapestSupply.SupplyQuantity, out int qty) ? qty : 0;

            var cartSession = HttpContext.Session.GetString("UserCart");
            List<CartItem> cartItems = string.IsNullOrEmpty(cartSession)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(cartSession) ?? new List<CartItem>();

            var existingItem = cartItems.FirstOrDefault(i => i.BookSupplyId == cheapestSupply.BookSupplyId);
            if (existingItem != null)
            {
                existingItem.Quantity += 1;
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
                    BookId = book.BookId,
                    BookName = book.BookName,
                    BookSupplyId = cheapestSupply.BookSupplyId,
                    SupplierName = cheapestSupply.SupplierKeyNavigation?.SupplierName ?? "Неизвестный",
                    PricePerUnit = cheapestSupply.BookSupplyPiecePrice * 1.6m,
                    Quantity = 1,
                    MaxAvailable = available,
                    SupplyDate = cheapestSupply.SupplyDate
                });
            }

            HttpContext.Session.SetString("UserCart", JsonSerializer.Serialize(cartItems));

            TempData["CartMessage"] = $"Книга \"{book.BookName}\" добавлена в корзину!";
            return RedirectToPage();
        }

        public class CartItem
        {
            public string CartItemId { get; set; } = Guid.NewGuid().ToString();
            public int BookId { get; set; }
            public string BookName { get; set; } = string.Empty;
            public int BookSupplyId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public DateOnly? SupplyDate { get; set; }
            public decimal PricePerUnit { get; set; }
            public int Quantity { get; set; }
            public int MaxAvailable { get; set; }
            public decimal TotalPrice => PricePerUnit * Quantity;
        }
    }
}