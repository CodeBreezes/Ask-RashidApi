using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BookingAppAPI.DB.Models.Address
{
    [Table("States")]
    public class State
    {
        [Key]
        public int UniqueId { get; set; }
        public int CountryId { get; set; }
        [ForeignKey("CountryId")]
        public Country Country { get; set; }
        public string Name { get; set; }
    }
}
