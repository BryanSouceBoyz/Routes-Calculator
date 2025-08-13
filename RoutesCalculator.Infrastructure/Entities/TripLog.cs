namespace RoutesCalculator.Infrastructure.Entities
{
    public class TripLog
    {
        public int Id { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Entrada
        public decimal DistanceKm { get; set; }
        public decimal DurationMinutes { get; set; }
        public decimal PublicCarFarePerRide { get; set; }
        public int PublicCarRidesCount { get; set; }
        public decimal UberBaseFare { get; set; }
        public decimal UberPerKm { get; set; }
        public decimal UberPerMinute { get; set; }
        public decimal UberServiceFee { get; set; }
        public decimal UberSurgeMultiplier { get; set; }
        public decimal FuelPricePerGallon { get; set; }
        public decimal CarEfficiencyKmPerGallon { get; set; }
        public decimal Tolls { get; set; }
        public decimal Parking { get; set; }
        public decimal MaintenancePerKm { get; set; }

        // Salida resumen
        public string BestMode { get; set; } = "";
        public decimal BestCost { get; set; }
        public string? OptionsJson { get; set; }
    }
}
