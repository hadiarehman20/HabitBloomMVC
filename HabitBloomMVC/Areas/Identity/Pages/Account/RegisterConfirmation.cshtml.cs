using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HabitBloomMVC.Areas.Identity.Pages.Account
{
    public class RegisterConfirmationModel : PageModel
    {
        public bool DisplayConfirmAccountLink { get; set; } = false;

        public string? EmailConfirmationUrl { get; set; }

        public void OnGet()
        {
        }
    }
}