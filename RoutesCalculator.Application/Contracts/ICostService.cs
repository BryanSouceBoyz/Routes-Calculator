using RoutesCalculator.Domain.Entities;

namespace RoutesCalculator.Application.Contracts
{

    public interface ICostService
    {

        IEnumerable<TripResult> CalculateAll(TripInput input);


        TripResult GetBest(TripInput input);
    }
}
