using System.ComponentModel.DataAnnotations;

namespace RoutesCalculator.Domain.Entities
{
    public class TripInput
    {
        [Required, Range(0.001, double.MaxValue, ErrorMessage = "DistanceKm must be > 0.")]
        public double DistanceKm { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "DurationMinutes cannot be negative.")]
        public int DurationMinutes { get; set; }

        // Carro público
        [Range(0, double.MaxValue, ErrorMessage = "PublicCarFarePerRide must be >= 0.")]
        public double PublicCarFarePerRide { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "PublicCarRidesCount must be >= 0.")]
        public int PublicCarRidesCount { get; set; }

        // Uber
        [Range(0, double.MaxValue, ErrorMessage = "UberBaseFare must be >= 0.")]
        public double UberBaseFare { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "UberPerKm must be >= 0.")]
        public double UberPerKm { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "UberPerMinute must be >= 0.")]
        public double UberPerMinute { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "UberServiceFee must be >= 0.")]
        public double UberServiceFee { get; set; }

        // si no quieres permitir < 1.0, usa Range(1, ...)
        [Range(0, double.MaxValue, ErrorMessage = "UberSurgeMultiplier must be >= 0.")]
        public double UberSurgeMultiplier { get; set; } = 1.0;

        // Carro propio
        [Range(0, double.MaxValue, ErrorMessage = "FuelPricePerGallon must be >= 0.")]
        public double FuelPricePerGallon { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "CarEfficiencyKmPerGallon must be > 0.")]
        public double CarEfficiencyKmPerGallon { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Tolls must be >= 0.")]
        public double Tolls { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Parking must be >= 0.")]
        public double Parking { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "MaintenancePerKm must be >= 0.")]
        public double MaintenancePerKm { get; set; }
    }
}

