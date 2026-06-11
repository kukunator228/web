using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using web.Models;

namespace web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ElochniBookStore2Context _context;

        public RegisterModel(ElochniBookStore2Context context)
        {
            _context = context;
        }

        [BindProperty]
        public string Login { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (_context.Users == null)
            {
                ErrorMessage = "Ошибка базы данных";
                return Page();
            }

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(Login))
            {
                ErrorMessage = "Введите логин!";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Введите электронную почту!";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите пароль!";
                return Page();
            }

            // Проверка совпадения паролей
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают!";
                return Page();
            }

            // Проверка длины пароля
            if (Password.Length < 6)
            {
                ErrorMessage = "Пароль должен содержать не менее 6 символов!";
                return Page();
            }

            // Проверка корректности email
            try
            {
                var addr = new System.Net.Mail.MailAddress(Email);
                if (addr.Address != Email)
                {
                    throw new Exception();
                }
            }
            catch
            {
                ErrorMessage = "Введите корректный email адрес!";
                return Page();
            }

            // Проверка существования логина
            var loginExists = await _context.Users.AnyAsync(u => u.UserLogIn == Login);
            if (loginExists)
            {
                ErrorMessage = "Пользователь с таким логином уже существует!";
                return Page();
            }

            // Проверка существования email
            var emailExists = await _context.Users.AnyAsync(u => u.UserEmail == Email);
            if (emailExists)
            {
                ErrorMessage = "Пользователь с таким email уже существует!";
                return Page();
            }

            // Создание нового пользователя
            var newUser = new User
            {
                UserLogIn = Login,
                UserEmail = Email,
                UserPassword = Password,
                RoleKey = 3
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // АВТОМАТИЧЕСКИЙ ВХОД ПОСЛЕ РЕГИСТРАЦИИ
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, newUser.UserLogIn),
                new Claim(ClaimTypes.Role, "Пользователь"),
                new Claim("UserID", newUser.UserId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Перенаправляем на главную страницу
            return RedirectToPage("/Index");
        }
    }
}