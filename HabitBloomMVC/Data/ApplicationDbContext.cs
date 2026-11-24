using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HabitBloomMVC.Models;

namespace HabitBloomMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitCompletion> HabitCompletions { get; set; }
    }
}