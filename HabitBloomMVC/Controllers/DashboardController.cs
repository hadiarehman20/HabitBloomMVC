
using HabitBloomMVC.Data;
using HabitBloomMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitBloomMVC.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            // GET USER'S ACTIVE HABITS
            var habits = await _context.Habits
                .Where(h => h.UserId == userId && h.IsActive)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);

            // GET USER'S HABIT COMPLETIONS
            var completions = await _context.HabitCompletions
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.HabitId,
                    c.CompletedDate
                })
                .ToListAsync();

            // CALENDAR COMPLETION DATES
            ViewBag.CompletedDates = completions
                .Select(c => c.CompletedDate.ToString("yyyy-MM-dd"))
                .Distinct()
                .ToList();

            // TODAY'S COMPLETED HABITS
            var completedHabitIds = completions
                .Where(c => c.CompletedDate == today)
                .Select(c => c.HabitId)
                .Distinct()
                .ToList();

            // TOTAL HABITS AND TODAY'S COMPLETIONS
            ViewBag.TotalHabits = habits.Count;
            ViewBag.CompletedToday = completedHabitIds.Count;
            ViewBag.CompletedHabitIds = completedHabitIds;

            // CURRENT STREAK
            var completedDates = completions
                .Select(c => c.CompletedDate)
                .Distinct()
                .ToHashSet();

            int currentStreak = 0;

            var streakDate = completedDates.Contains(today)
                ? today
                : today.AddDays(-1);

            while (completedDates.Contains(streakDate))
            {
                currentStreak++;
                streakDate = streakDate.AddDays(-1);
            }

            ViewBag.CurrentStreak = currentStreak;

            // WEEKLY PROGRESS (MONDAY TO SUNDAY)
            int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;

            var weekStart = today.AddDays(-daysSinceMonday);

            var weeklyProgress = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                var currentDate = weekStart.AddDays(i);

                int completedCount = completions
                    .Where(c => c.CompletedDate == currentDate)
                    .Select(c => c.HabitId)
                    .Distinct()
                    .Count();

                int percentage = habits.Count > 0
                    ? (int)Math.Round(
                        (double)completedCount / habits.Count * 100)
                    : 0;

                weeklyProgress.Add(Math.Min(percentage, 100));
            }

            ViewBag.WeeklyProgress = weeklyProgress;

            return View(habits);
        }


        // POST: Dashboard/Complete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h =>
                    h.HabitId == id &&
                    h.UserId == userId &&
                    h.IsActive);

            if (habit == null)
            {
                return NotFound();
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            var alreadyCompleted = await _context.HabitCompletions
                .AnyAsync(c =>
                    c.HabitId == id &&
                    c.UserId == userId &&
                    c.CompletedDate == today);

            if (!alreadyCompleted)
            {
                _context.HabitCompletions.Add(new HabitCompletion
                {
                    HabitId = id,
                    UserId = userId,
                    CompletedDate = today
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // POST: Dashboard/UndoComplete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UndoComplete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            var completion = await _context.HabitCompletions
                .FirstOrDefaultAsync(c =>
                    c.HabitId == id &&
                    c.UserId == userId &&
                    c.CompletedDate == today);

            if (completion != null)
            {
                _context.HabitCompletions.Remove(completion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
