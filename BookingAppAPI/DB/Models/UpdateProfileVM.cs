using System.ComponentModel.DataAnnotations;

namespace BookingAppAPI.DB.Models
{
    public class UpdateProfileVM
    {
         public int UserID { get; set; }
        public string? FirstName { get; set; } = string.Empty;
 
        public string? LastName { get; set; } = string.Empty;

        public string? Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string? Address { get; set; } = string.Empty;

        public string? Gender { get; set; } = string.Empty;

        public IFormFile? ProfileImage { get; set; }  
    }

}
