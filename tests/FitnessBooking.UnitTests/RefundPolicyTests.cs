using FitnessBooking.Application;
using FluentAssertions;

namespace FitnessBooking.UnitTests;

public class RefundPolicyTests
{
    [TestCase(100, 24, 100)]
    [TestCase(100, 10, 50)]
    [TestCase(100, 2, 50)]
    [TestCase(100, 1, 0)]
    public void Refund_is_correct_by_window(decimal price, int hoursBefore, decimal expected)
    {
        var policy = new RefundPolicy();
        var start = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancel = start.AddHours(-hoursBefore);

        var refund = policy.GetRefundAmount(price, start, cancel);

        refund.Should().Be(expected);
    }

    [Test]
    public void Refund_after_class_start_is_zero()
    {
        var policy = new RefundPolicy();
        var start = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancel = start.AddMinutes(1);

        policy.GetRefundAmount(100m, start, cancel).Should().Be(0m);
    }
}
