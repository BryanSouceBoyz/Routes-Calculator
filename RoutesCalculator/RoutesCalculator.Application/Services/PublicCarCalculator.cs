using RoutesCalculator.Application.Contracts;
using RoutesCalculator.Domain.Entities;
using RoutesCalculator.Domain.Enums;

namespace RoutesCalculator.Application.Services
{

    public class PublicCarCalculator : ICostCalculator
    {
        public TransportMode Mode => TransportMode.PublicCar;

        public TripResult Calculate(TripInput input)
        {
            var total = input.PublicCarFarePerRide * input.PublicCarRidesCount;

            return new TripResult
            {
                Mode = Mode,
                TotalCost = total,
                Breakdown = $"Tramos: {input.PublicCarRidesCount} × {input.PublicCarFarePerRide:0.00} = {total:0.00}"
            };
        }
    }
}
