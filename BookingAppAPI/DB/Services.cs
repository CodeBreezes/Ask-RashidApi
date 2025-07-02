using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingAppAPI.DB
{
    public class Services
    {
        [Key]
        public int UniqueId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Cost { get; set; }
        [NotMapped]
        public IFormFile? Image { get; set; }

        public string ImagePath { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; } 
        public bool IsActive { get; set; } = true;
    }
}
