using System.ComponentModel.DataAnnotations;

namespace BookingAppAPI.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
