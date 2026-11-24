
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HabitBloomMVC.Models
{
    public class HabitCompletion
    {
        public int HabitCompletionId { get; set; }

        [Required]
        public int HabitId { get; set; }

        [ForeignKey(nameof(HabitId))]
        public Habit Habit { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateOnly CompletedDate { get; set; }
    }
}
