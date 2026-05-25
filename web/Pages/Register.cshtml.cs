using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;

        public RegisterModel(AppDbContext context)
        {
            _context = context;
        }

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(string login, string password)
        {
            if (_context.Users == null) return Page();

            var exists = await _context.Users.AnyAsync(u => u.UserLogIn == login);
            if (exists)
            {
                ErrorMessage = "ѕользователь с таким логином уже существует!";
                return Page();
            }

            var newUser = new User
            {
                UserLogIn = login,
                UserPasssword = password,
                RoleKey = 3
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login");
        }
    }
}