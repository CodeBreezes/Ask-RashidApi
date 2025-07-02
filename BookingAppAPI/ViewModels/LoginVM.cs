using System.ComponentModel.DataAnnotations;

namespace Bpst.API.ViewModels
{
    public class LoginVM
    {
        [Required]
        [Display(Name = "Login Name")]
        public string LoginName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
