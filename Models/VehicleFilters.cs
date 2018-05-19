using System.ComponentModel.DataAnnotations;

namespace vehicles_api.Models
{
    public class VehicleFilters
    {
        [Range(1950, 2050)]
        public int? Year { get; set; }

        [Range(1950, 2050)]
        public int? MinYear { get; set; }

        [Range(1950, 2050)]
        public int? MaxYear { get; set; }

        public string Make { get; set; }

        public string MakeContains { get; set; }

        public string Model { get; set; }

        public string ModelContains { get; set; }

        internal bool IsEmpty
        {
            get
            {
                return !Year.HasValue &&
                    !MinYear.HasValue &&
                    !MaxYear.HasValue &&
                    string.IsNullOrEmpty(Make) &&
                    string.IsNullOrEmpty(MakeContains) &&
                    string.IsNullOrEmpty(Model) &&
                    string.IsNullOrEmpty(ModelContains);
            }
        }
    }
}