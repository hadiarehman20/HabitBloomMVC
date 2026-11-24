using System.ComponentModel.DataAnnotations;

namespace HabitBloomMVC.Models
{
    public class Habit
    {
        public int HabitId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "General";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}