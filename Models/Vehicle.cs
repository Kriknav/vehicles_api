

using System.ComponentModel.DataAnnotations;

namespace vehicles_api.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        [Required]
        [Range(1950, 2050)]
        public int Year { get; set; }
        [Required]
        public string Make { get; set; }
        [Required]
        public string Model { get; set; }
    }
}