using RoutesCalculator.Application.Contracts;
using RoutesCalculator.Domain.Entities;

namespace RoutesCalculator.Application.Services
{

    public class CostService : ICostService
    {
        private readonly IEnumerable<ICostCalculator> _calculators;

        public CostService(IEnumerable<ICostCalculator> calculators)
        {
            _calculators = calculators;
        }

        public IEnumerable<TripResult> CalculateAll(TripInput input)
            => _calculators.Select(c => c.Calculate(input))
                           .OrderBy(r => r.TotalCost);

        public TripResult GetBest(TripInput input)
            => CalculateAll(input).First();
    }
}
