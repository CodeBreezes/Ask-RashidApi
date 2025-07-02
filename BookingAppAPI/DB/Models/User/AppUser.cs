using System.ComponentModel.DataAnnotations;

namespace BookingAppAPI.DB.Models.User
{
    public class AppUser
    {
        [Key]
        [Display(Name = "Unique ID")]
        public int UniqueId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";

        [MaxLength(15)]
        [Phone]
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [MaxLength(50)]
        public string? Gender { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; } 


        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string LoginEmail { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public List<string>? Roles { get; set; } = [];
    }
}
