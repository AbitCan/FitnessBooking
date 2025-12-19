using FitnessBooking.Application;
using FitnessBooking.Domain;
using NUnit.Framework;

namespace FitnessBooking.UnitTests;

public class PairwisePricingRefundTests
{
    // Derived from tools/pairwise/pairwise_cases.txt (PICT output)

    [TestCase(MembershipType.Premium, TimeSlot.OffPeak, OccupancyBand.High, "GE24H", 104.00, 104.00)]
    [TestCase(MembershipType.Student, TimeSlot.Peak, OccupancyBand.Low, "H2_24H", 84.00, 42.00)]
    [TestCase(MembershipType.Student, TimeSlot.OffPeak, OccupancyBand.Mid, "GE24H", 77.00, 77.00)]
    [TestCase(MembershipType.Standard, TimeSlot.Peak, OccupancyBand.High, "LT2H", 156.00, 0.00)]
    [TestCase(MembershipType.Standard, TimeSlot.OffPeak, OccupancyBand.Mid, "H2_24H", 110.00, 55.00)]
    [TestCase(MembershipType.Premium, TimeSlot.Peak, OccupancyBand.High, "H2_24H", 124.80, 62.40)]
    [TestCase(MembershipType.Premium, TimeSlot.OffPeak, OccupancyBand.Low, "LT2H", 80.00, 0.00)]
    [TestCase(MembershipType.Premium, TimeSlot.Peak, OccupancyBand.Mid, "LT2H", 105.60, 0.00)]
    [TestCase(MembershipType.Student, TimeSlot.Peak, OccupancyBand.High, "LT2H", 109.20, 0.00)]
    [TestCase(MembershipType.Standard, TimeSlot.Peak, OccupancyBand.Low, "GE24H", 120.00, 120.00)]
    public void Pairwise_case_produces_expected_price_and_refund(
        MembershipType membership,
        TimeSlot timeSlot,
        OccupancyBand occupancy,
        string cancelWindow,
        double expectedPrice,
        double expectedRefund)
    {
        var pricing = new PricingCalculator();
        var refundPolicy = new RefundPolicy();

        var startUtc = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var cancelUtc = cancelWindow switch
        {
            "GE24H" => startUtc.AddHours(-24),
            "H2_24H" => startUtc.AddHours(-10), // safely inside 2..24h window
            "LT2H" => startUtc.AddHours(-1),
            _ => throw new ArgumentOutOfRangeException(nameof(cancelWindow))
        };

        var price = pricing.CalculatePrice(membership, timeSlot, occupancy);
        var refund = refundPolicy.GetRefundAmount(price, startUtc, cancelUtc);

        Assert.That(price, Is.EqualTo((decimal)expectedPrice));
        Assert.That(refund, Is.EqualTo((decimal)expectedRefund));
    }
}
