using RoutesCalculator.Application.Contracts;
using RoutesCalculator.Domain.Entities;
using RoutesCalculator.Domain.Enums;

namespace RoutesCalculator.Application.Services
{

    public class OwnCarCalculator : ICostCalculator
    {
        public TransportMode Mode => TransportMode.OwnCar;

        public TripResult Calculate(TripInput input)
        {
            var fuelCost = (input.DistanceKm / input.CarEfficiencyKmPerGallon) * input.FuelPricePerGallon;
            var maintenance = input.MaintenancePerKm * input.DistanceKm;
            var total = fuelCost + input.Tolls + input.Parking + maintenance;

            return new TripResult
            {
                Mode = Mode,
                TotalCost = total,
                Breakdown =
                    $"Combustible:{fuelCost:0.00} + Peajes:{input.Tolls:0.00} + Parqueo:{input.Parking:0.00} + Manto:{maintenance:0.00}"
            };
        }
    }
}
