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
}
