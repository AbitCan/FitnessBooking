using FitnessBooking.Domain;

namespace FitnessBooking.Application;

public sealed class PricingCalculator
{
    public decimal CalculatePrice(MembershipType membership, TimeSlot timeSlot, OccupancyBand occupancy)
    {
        var basePrice = membership switch
        {
            MembershipType.Standard => 100m,
            MembershipType.Premium => 80m,
            MembershipType.Student => 70m,
            _ => throw new ArgumentOutOfRangeException(nameof(membership))
        };

        var timeMultiplier = timeSlot switch
        {
            TimeSlot.OffPeak => 1.00m,
            TimeSlot.Peak => 1.20m,
            _ => throw new ArgumentOutOfRangeException(nameof(timeSlot))
        };

        var occupancyMultiplier = occupancy switch
        {
            OccupancyBand.Low => 1.00m,
            OccupancyBand.Mid => 1.10m,
            OccupancyBand.High => 1.30m,
            _ => throw new ArgumentOutOfRangeException(nameof(occupancy))
        };

        var price = basePrice * timeMultiplier * occupancyMultiplier;
        return Math.Round(price, 2, MidpointRounding.AwayFromZero);
    }
}
