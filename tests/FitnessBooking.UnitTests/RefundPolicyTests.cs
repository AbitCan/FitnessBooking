using FitnessBooking.Application;
using NUnit.Framework;
using System;

namespace FitnessBooking.UnitTests;

public class RefundPolicyTests
{
    [Test]
    public void GetRefundAmount_throws_when_pricePaid_is_negative()
    {
        var policy = new RefundPolicy();

        var startUtc = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancelUtc = startUtc.AddHours(-5);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            policy.GetRefundAmount(-1m, startUtc, cancelUtc));
    }

    [Test]
    public void GetRefundAmount_returns_zero_when_cancel_is_after_class_start()
    {
        var policy = new RefundPolicy();

        var startUtc = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancelUtc = startUtc.AddMinutes(1); // after start

        var refund = policy.GetRefundAmount(100m, startUtc, cancelUtc);

        Assert.That(refund, Is.EqualTo(0m));
    }
    [Test]
    public void GetRefundAmount_full_refund_at_exactly_24_hours()
    {
        var policy = new RefundPolicy();
        var startUtc = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancelUtc = startUtc.AddHours(-24);

        var refund = policy.GetRefundAmount(100m, startUtc, cancelUtc);

        Assert.That(refund, Is.EqualTo(100m));
    }

    [Test]
    public void GetRefundAmount_half_refund_just_under_24_hours()
    {
        var policy = new RefundPolicy();
        var startUtc = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancelUtc = startUtc.AddHours(-24).AddTicks(1); // delta = 23:59:59.9999999

        var refund = policy.GetRefundAmount(100m, startUtc, cancelUtc);

        Assert.That(refund, Is.EqualTo(50m));
    }

    [Test]
    public void GetRefundAmount_half_refund_at_exactly_2_hours()
    {
        var policy = new RefundPolicy();
        var startUtc = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancelUtc = startUtc.AddHours(-2);

        var refund = policy.GetRefundAmount(100m, startUtc, cancelUtc);

        Assert.That(refund, Is.EqualTo(50m));
    }

    [Test]
    public void GetRefundAmount_zero_refund_just_under_2_hours()
    {
        var policy = new RefundPolicy();
        var startUtc = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancelUtc = startUtc.AddHours(-2).AddTicks(1); // delta = 1:59:59.9999999

        var refund = policy.GetRefundAmount(100m, startUtc, cancelUtc);

        Assert.That(refund, Is.EqualTo(0m));
    }

}
