using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookingAppAPI.DB.Models;

namespace BookingAppAPI.DB
{
    public class Services
    {
        [Key]
        public int UniqueId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int Cost { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; } 
        public bool IsActive { get; set; } = true;
        public ICollection<Subtopics> Subtopics { get; set; } = new List<Subtopics>();
    }
}
