

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace vehicles_api.Models
{
    public class Vehicle
    {
        [JsonProperty("Id")]
        public int Id { get; set; }
        [Required]
        [Range(1950, 2050)]
        [JsonProperty("Year")]
        public int Year { get; set; }
        [Required]
        [JsonProperty("Make")]
        public string Make { get; set; }
        [Required]
        [JsonProperty("Model")]
        public string Model { get; set; }
    }
}