using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BookingAppAPI.DB.Models
{
    public class Subtopics
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        [JsonIgnore]
        public Services? Service { get; set; } 
        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public ICollection<Bulletins> Bulletins { get; set; } = new List<Bulletins>();

    }
}
