using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web;

namespace web.Pages
{
    public class AuthorsModel : PageModel
    {
        private readonly AppDbContext _context;

        public AuthorsModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Author> Authors { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public async Task OnGetAsync()
        {
            if (_context.Authors != null)
            {
                var authorsQuery = from a in _context.Authors select a;

                if (!string.IsNullOrEmpty(SearchString))
                {
                    string search = SearchString.Trim();

                    authorsQuery = authorsQuery.Where(a =>
                        EF.Functions.Like(a.AuthorSecondName, $"%{search}%") ||
                        EF.Functions.Like(a.AuthorFirstName, $"%{search}%") ||
                        (a.AuthorPatronymic != null && EF.Functions.Like(a.AuthorPatronymic, $"%{search}%"))
                    );
                }

                Authors = await authorsQuery.ToListAsync();
            }
        }
    }
}