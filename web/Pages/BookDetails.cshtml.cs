using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace web.Pages
{
    public class BookDetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public BookDetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Book? CurrentBook { get; set; }
        public List<Author> AuthorsList { get; set; } = new List<Author>();
        public bool IsAddedSuccess { get; set; } = false;

        // Список отзывов для отображения на странице
        public List<BookReview> ReviewsList { get; set; } = new List<BookReview>();

        // Свойство для связывания нового текста отзыва из формы
        [BindProperty]
        public string NewReviewText { get; set; } = string.Empty;

        // Свойство для связывания числовой оценки (1-5) из формы
        [BindProperty]
        public int NewReviewScore { get; set; } = 5; // По умолчанию ставим 5 звезд

        // НОВОЕ: Свойство для хранения средней оценки книги
        public double AverageScore { get; set; } = 0.0;

        public string ReviewErrorMessage { get; set; } = string.Empty;

        // Загрузка всех данных книги и отзывов
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Books == null) return Page();

            CurrentBook = await _context.Books
                .Include(b => b.BookType)
                .FirstOrDefaultAsync(b => b.BookID == id);

            if (CurrentBook != null)
            {
                // Подгружаем авторов произведения
                if (_context.BookAuthors != null)
                {
                    AuthorsList = await _context.BookAuthors
                        .Where(ba => ba.BookID == CurrentBook.BookID)
                        .Select(ba => ba.Author)
                        .Where(a => a != null)
                        .ToListAsync();
                }

                // Подгружаем отзывы, сортируя их: самые полезные (высокий рейтинг) будут сверху!
                if (_context.BookReviews != null)
                {
                    ReviewsList = await _context.BookReviews
                        .Include(r => r.User) // Подтягиваем данные автора отзыва
                        .Where(r => r.BookKey == CurrentBook.BookID)
                        .OrderByDescending(r => r.RatingSum) // Сортировка по полезности
                        .ThenByDescending(r => r.ReviewDate) // При равных баллах - сначала новые
                        .ToListAsync();

                    // НОВОЕ: Расчет средней оценки на основе BookScore
                    if (ReviewsList != null && ReviewsList.Any())
                    {
                        // Считаем среднее значение и округляем до 1 знака после запятой
                        AverageScore = Math.Round(ReviewsList.Average(r => r.BookScore), 1);
                    }
                    else
                    {
                        AverageScore = 0.0; // Если отзывов и оценок ещё нет
                    }
                }
            }

            return Page();
        }

        // Старый метод добавления в корзину
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.Books == null) return Page();

            await OnGetAsync(id);

            var cartSession = HttpContext.Session.GetString("UserCart");
            List<int> cartItems = string.IsNullOrEmpty(cartSession)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(cartSession) ?? new List<int>();

            cartItems.Add(id.Value);

            HttpContext.Session.SetString("UserCart", JsonSerializer.Serialize(cartItems));

            IsAddedSuccess = true;
            return Page();
        }

        // МЕТОД: Добавление нового отзыва на книгу с оценкой
        public async Task<IActionResult> OnPostAddReviewAsync(int id)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                // Если произошла ошибка, подгружаем данные страницы заново, чтобы она не была пустой
                await OnGetAsync(id);
                ReviewErrorMessage = "Только зарегистрированные пользователи могут оставлять отзывы!";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewReviewText))
            {
                return RedirectToPage(new { id });
            }

            // Создаем отзыв и передаем туда NewReviewScore, полученный из формы
            var review = new BookReview
            {
                BookKey = id,
                UserKey = currentUserId,
                ReviewText = NewReviewText.Trim(),
                ReviewDate = DateTime.Now,
                RatingSum = 0,
                BookScore = NewReviewScore // Записываем оценку в созданный столбец!
            };

            _context.BookReviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }

        // МЕТОД: Голосование за отзыв (Стрелочки вверх/вниз)
        public async Task<IActionResult> OnPostVoteAsync(int id, int reviewId, int voteValue)
        {
            var userIdClaim = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return RedirectToPage(new { id }); // Если не залогинен - игнорируем клик
            }

            // Ищем, голосовал ли этот юзер за этот отзыв ранее
            var existingVote = await _context.ReviewVotes
                .FirstOrDefaultAsync(v => v.ReviewKey == reviewId && v.UserKey == currentUserId);

            var review = await _context.BookReviews.FirstOrDefaultAsync(r => r.ReviewID == reviewId);
            if (review == null) return RedirectToPage(new { id });

            if (existingVote != null)
            {
                // Если пользователь нажал ту же стрелочку, что и раньше — отменяем его голос (убираем лайк/дизлайк)
                if (existingVote.VoteValue == voteValue)
                {
                    review.RatingSum -= voteValue; // Возвращаем рейтинг обратно
                    _context.ReviewVotes.Remove(existingVote);
                }
                else
                {
                    // Если пользователь передумал и нажал противоположную стрелочку
                    review.RatingSum += (voteValue * 2); // Меняем баланс рейтинга (с -1 до +1 разница в 2 балла)
                    existingVote.VoteValue = voteValue;
                }
            }
            else
            {
                // Новое голосование
                var vote = new ReviewVote
                {
                    ReviewKey = reviewId,
                    UserKey = currentUserId,
                    VoteValue = voteValue
                };
                _context.ReviewVotes.Add(vote);
                review.RatingSum += voteValue;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id });
        }
    }
}