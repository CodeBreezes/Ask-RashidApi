using System.ComponentModel.DataAnnotations;

namespace BookingAppAPI.DB.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        public int userId { get; set; }
        public string? Name { get; set; }
 
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public string? Category { get; set; }

        public string? Message { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

    public enum ContactCategory
    {
        BrandCollaboration,
        FeedbackSuggestions,
        ComplaintIssue,
        GeneralEnquiry
    }

}
