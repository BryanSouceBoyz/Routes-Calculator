using RoutesCalculator.Domain.Entities;
using RoutesCalculator.Domain.Enums;

namespace RoutesCalculator.Application.Contracts
{

    public interface ICostCalculator
    {

        TransportMode Mode { get; }


        TripResult Calculate(TripInput input);
    }
}
