using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingAppAPI.DB.Models.Address
{
    public class Address
    {
        [Key]
        [Display(Name = "Unique ID")]
        public int UniqueId { get; set; }


        [Required]
        [Display(Name = "Address Line 1 ")]
        public string AddressLine1 { get; set; } = string.Empty;

        [Display(Name = "Address Line 2 ")]
        public string AddressLine2 { get; set; } = string.Empty;

        [Display(Name = "Address Line 3 ")]
        public string AddressLine3 { get; set; } = string.Empty;

        [Display(Name = "Nearest Landmark")]
        public string NearestLandMark { get; set; } = string.Empty;

        [Display(Name = "State")]
        public int? StateId { get; set; }
        public State? State { get; set; }


        [Display(Name = "City")]
        public int? CityId { get; set; }
        public City? City { get; set; }


        [Display(Name = "Country")]
        public int? CountryId { get; set; }
        public Country? Country { get; set; }


    }
}
