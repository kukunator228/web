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

        public async Task OnGetAsync()
        {
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
    }
}