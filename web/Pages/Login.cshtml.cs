using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.Models;

namespace web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ElochniBookStore2Context _context;

        public LoginModel(ElochniBookStore2Context context)
        {
            _context = context;
        }

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(string login, string password)
        {
            if (_context.Users == null) return Page();

            var user = await _context.Users
                .Include(u => u.RoleKeyNavigation)  // ИСПРАВЛЕНО: Role -> RoleKeyNavigation
                .FirstOrDefaultAsync(u => u.UserLogIn == login && u.UserPassword == password);  // ИСПРАВЛЕНО: UserPasssword -> UserPassword

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserLogIn),
                    new Claim(ClaimTypes.Role, user.RoleKeyNavigation?.RoleName ?? "Пользователь"),  // ИСПРАВЛЕНО: используем RoleKeyNavigation
                    
                    // ИСПРАВЛЕНО: UserID -> UserId
                    new Claim("UserID", user.UserId.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToPage("/Books");
            }

            ErrorMessage = "Неверный логин или пароль!";
            return Page();
        }
    }
}