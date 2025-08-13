using RoutesCalculator.Application.Contracts;
using RoutesCalculator.Domain.Entities;
using RoutesCalculator.Domain.Enums;

namespace RoutesCalculator.Application.Services
{

    public class UberCalculator : ICostCalculator
    {
        public TransportMode Mode => TransportMode.Uber;

        public TripResult Calculate(TripInput input)
        {
            var subtotal =
                  input.UberBaseFare
                + (input.UberPerKm * input.DistanceKm)
                + (input.UberPerMinute * input.DurationMinutes)
                + input.UberServiceFee;

            var surge = input.UberSurgeMultiplier <= 0 ? 1.0 : input.UberSurgeMultiplier;
            var total = subtotal * surge;

            return new TripResult
            {
                Mode = Mode,
                TotalCost = total,
                Breakdown =
                    $"Base:{input.UberBaseFare:0.00} + Dist:{input.UberPerKm * input.DistanceKm:0.00} + " +
                    $"Tiempo:{input.UberPerMinute * input.DurationMinutes:0.00} + Fee:{input.UberServiceFee:0.00} " +
                    $"→ x{surge:0.00} = {total:0.00}"
            };
        }
    }
}
