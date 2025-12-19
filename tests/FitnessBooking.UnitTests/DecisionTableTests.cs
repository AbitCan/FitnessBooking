using FitnessBooking.Application;
using FitnessBooking.Domain;
using NUnit.Framework;

namespace FitnessBooking.UnitTests;

public class DecisionTableTests
{
    // Derived from tools/decision-table/decision_tables.md (DT-1)
    [TestCase(MembershipType.Standard, TimeSlot.OffPeak, OccupancyBand.Low, 100.00)]
    [TestCase(MembershipType.Standard, TimeSlot.OffPeak, OccupancyBand.Mid, 110.00)]
    [TestCase(MembershipType.Standard, TimeSlot.OffPeak, OccupancyBand.High, 130.00)]
    [TestCase(MembershipType.Standard, TimeSlot.Peak, OccupancyBand.Low, 120.00)]
    [TestCase(MembershipType.Standard, TimeSlot.Peak, OccupancyBand.Mid, 132.00)]
    [TestCase(MembershipType.Standard, TimeSlot.Peak, OccupancyBand.High, 156.00)]

    [TestCase(MembershipType.Premium, TimeSlot.OffPeak, OccupancyBand.Low, 80.00)]
    [TestCase(MembershipType.Premium, TimeSlot.OffPeak, OccupancyBand.Mid, 88.00)]
    [TestCase(MembershipType.Premium, TimeSlot.OffPeak, OccupancyBand.High, 104.00)]
    [TestCase(MembershipType.Premium, TimeSlot.Peak, OccupancyBand.Low, 96.00)]
    [TestCase(MembershipType.Premium, TimeSlot.Peak, OccupancyBand.Mid, 105.60)]
    [TestCase(MembershipType.Premium, TimeSlot.Peak, OccupancyBand.High, 124.80)]

    [TestCase(MembershipType.Student, TimeSlot.OffPeak, OccupancyBand.Low, 70.00)]
    [TestCase(MembershipType.Student, TimeSlot.OffPeak, OccupancyBand.Mid, 77.00)]
    [TestCase(MembershipType.Student, TimeSlot.OffPeak, OccupancyBand.High, 91.00)]
    [TestCase(MembershipType.Student, TimeSlot.Peak, OccupancyBand.Low, 84.00)]
    [TestCase(MembershipType.Student, TimeSlot.Peak, OccupancyBand.Mid, 92.40)]
    [TestCase(MembershipType.Student, TimeSlot.Peak, OccupancyBand.High, 109.20)]
    public void DT1_PricingCalculator_matches_decision_table(
        MembershipType membership,
        TimeSlot timeSlot,
        OccupancyBand occupancy,
        double expected)
    {
        var calc = new PricingCalculator();
        var price = calc.CalculatePrice(membership, timeSlot, occupancy);

        Assert.That(price, Is.EqualTo((decimal)expected));
    }

    // Derived from tools/decision-table/decision_tables.md (DT-2)
    [TestCase("GE24H", 25, 1.00)]
    [TestCase("H2_24H", 10, 0.50)]
    [TestCase("LT2H", 1, 0.00)]
    public void DT2_RefundPolicy_matches_decision_table(string window, int hoursBefore, double fraction)
    {
        var policy = new RefundPolicy();

        var startUtc = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancelUtc = startUtc.AddHours(-hoursBefore);

        var pricePaid = 200m;
        var refund = policy.GetRefundAmount(pricePaid, startUtc, cancelUtc);

        var expectedRefund = pricePaid * (decimal)fraction;
        Assert.That(refund, Is.EqualTo(expectedRefund));
    }
}
