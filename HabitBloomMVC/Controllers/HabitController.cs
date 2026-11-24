
using HabitBloomMVC.Data;
using HabitBloomMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitBloomMVC.Controllers
{
    [Authorize]
    public class HabitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HabitController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        // GET: Habit
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var habits = await _context.Habits
                .Where(h => h.UserId == userId && h.IsActive)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);

            var completedHabitIds = await _context.HabitCompletions
                .Where(c => c.UserId == userId && c.CompletedDate == today)
                .Select(c => c.HabitId)
                .ToListAsync();

            ViewBag.CompletedHabitIds = completedHabitIds;

            return View(habits);
        }
        // GET: Habit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Habit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Habit habit)
        {
            ModelState.Remove(nameof(habit.UserId));

            if (!ModelState.IsValid)
            {
                return View(habit);
            }

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            habit.UserId = userId;
            habit.CreatedAt = DateTime.Now;
            habit.IsActive = true;

            _context.Habits.Add(habit);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Habit/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            return View(habit);
        }

        // POST: Habit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, string? description, string category)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Habit name is required.");
            }
            else if (name.Length > 100)
            {
                ModelState.AddModelError("Name", "Habit name cannot exceed 100 characters.");
            }

            if (description != null && description.Length > 300)
            {
                ModelState.AddModelError("Description", "Description cannot exceed 300 characters.");
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                ModelState.AddModelError("Category", "Please select a category.");
            }
            else if (category.Length > 50)
            {
                ModelState.AddModelError("Category", "Category cannot exceed 50 characters.");
            }

            if (!ModelState.IsValid)
            {
                habit.Name = name;
                habit.Description = description;
                habit.Category = category;

                return View(habit);
            }

            // Update only the editable fields
            habit.Name = name.Trim();
            habit.Description = description?.Trim();
            habit.Category = category;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Habit/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            return View(habit);
        }

        // POST: Habit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Challenge();
            }

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.HabitId == id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // POST: Habit/Complete
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
                .FirstOrDefaultAsync(h => h.HabitId == id && h.UserId == userId && h.IsActive);

            if (habit == null)
            {
                return NotFound();
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            var alreadyCompleted = await _context.HabitCompletions
                .AnyAsync(c => c.HabitId == id
                             && c.UserId == userId
                             && c.CompletedDate == today);

            if (!alreadyCompleted)
            {
                var completion = new HabitCompletion
                {
                    HabitId = id,
                    UserId = userId,
                    CompletedDate = today
                };

                _context.HabitCompletions.Add(completion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Habit");
        }

        // POST: Habit/UndoComplete
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
                .FirstOrDefaultAsync(c => c.HabitId == id
                                     && c.UserId == userId
                                     && c.CompletedDate == today);

            if (completion != null)
            {
                _context.HabitCompletions.Remove(completion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Habit");
        }

    }
}
