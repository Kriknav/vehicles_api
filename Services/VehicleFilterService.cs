using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using vehicles_api.Models;

namespace vehicles_api.Filters
{
    public interface IVehicleFilterService
    {
        List<Vehicle> GetVehicles(VehicleFilters filteringParams, VehicleContext context);
    }

    public class VehicleFilterService : IVehicleFilterService
    {
        public List<Vehicle> GetVehicles(VehicleFilters filteringParams, VehicleContext context)
        {
            var query = context.Vehicles.AsQueryable();

            if (!filteringParams.IsEmpty)
            {
                // We were presented with filters, so let's check them and add them to the
                // WHERE clause as needed.

                // Consider year filters first.
                //   If the 'YEAR ===' filter is used it takes precedence
                //   Otherwise consider some combination of the 'YEAR >=' and 'YEAR <=' filters
                if (filteringParams.Year.HasValue)
                {
                    query = query.Where(v => v.Year == filteringParams.Year.Value);
                }
                else if (filteringParams.MinYear.HasValue || filteringParams.MaxYear.HasValue)
                {
                    // we use two if statements instead of if/else to allow for any combination of these two filters
                    if (filteringParams.MinYear.HasValue)
                    {
                        query = query.Where(v => v.Year >= filteringParams.MinYear.Value);
                    }

                    if (filteringParams.MaxYear.HasValue)
                    {
                        query = query.Where(v => v.Year <= filteringParams.MaxYear.Value);
                    }
                }

                // For Make and Model filters, if they ask for an exact match, that takes precedence,
                //   otherwise we use partial matching if they provided such a filter
                if (!string.IsNullOrEmpty(filteringParams.Make))
                {
                    query = query.Where(v => v.Make.ToLowerInvariant() == filteringParams.Make.ToLowerInvariant());
                }
                else if (!string.IsNullOrEmpty(filteringParams.MakeContains))
                {
                    query = query.Where(v => v.Make.ToLowerInvariant().Contains(filteringParams.MakeContains.ToLowerInvariant()));
                }

                if (!string.IsNullOrEmpty(filteringParams.Model))
                {
                    query = query.Where(v => v.Model.ToLowerInvariant() == filteringParams.Model.ToLowerInvariant());
                }
                else if (!string.IsNullOrEmpty(filteringParams.ModelContains))
                {
                    query = query.Where(v => v.Model.ToLowerInvariant().Contains(filteringParams.ModelContains.ToLowerInvariant()));
                }
            }

            return query.ToList();
        }
    }
}
