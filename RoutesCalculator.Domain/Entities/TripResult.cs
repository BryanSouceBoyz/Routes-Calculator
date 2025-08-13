using System;
using RoutesCalculator.Domain.Enums;

namespace RoutesCalculator.Domain.Entities
{
 
    public class TripResult
    {

        public TransportMode Mode { get; set; }


        public double TotalCost { get; set; }


        public string Breakdown { get; set; } = string.Empty;
    }
}
