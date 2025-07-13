using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookingAppAPI.DB.Models
{
    public class Bulletins
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        public int SubtopicId { get; set; }
        [ForeignKey("SubtopicId")]
        public Subtopics? Subtopic { get; set; } 

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }
    }

}
