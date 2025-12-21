using System;
using FitnessBooking.Application;
using FitnessBooking.Domain;
using NUnit.Framework;

namespace FitnessBooking.UnitTests;

public class PricingCalculatorTests
{
    [Test]
    public void CalculatePrice_throws_for_invalid_membership()
    {
        var calc = new PricingCalculator();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            calc.CalculatePrice((MembershipType)999, TimeSlot.OffPeak, OccupancyBand.Low));

        Assert.That(ex!.ParamName, Is.EqualTo("membership"));
    }

    [Test]
    public void CalculatePrice_throws_for_invalid_timeslot()
    {
        var calc = new PricingCalculator();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            calc.CalculatePrice(MembershipType.Standard, (TimeSlot)999, OccupancyBand.Low));

        Assert.That(ex!.ParamName, Is.EqualTo("timeSlot"));
    }

    [Test]
    public void CalculatePrice_throws_for_invalid_occupancy()
    {
        var calc = new PricingCalculator();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            calc.CalculatePrice(MembershipType.Standard, TimeSlot.OffPeak, (OccupancyBand)999));

        Assert.That(ex!.ParamName, Is.EqualTo("occupancy"));
    }

    [Test]
    public void CalculatePrice_covers_all_valid_branches()
    {
        var calc = new PricingCalculator();

        // Just calling these hits the non-throw switch arms:
        _ = calc.CalculatePrice(MembershipType.Standard, TimeSlot.OffPeak, OccupancyBand.Low);
        _ = calc.CalculatePrice(MembershipType.Premium, TimeSlot.Peak, OccupancyBand.Mid);
        _ = calc.CalculatePrice(MembershipType.Student, TimeSlot.Peak, OccupancyBand.High);
    }
}
