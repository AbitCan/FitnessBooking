namespace FitnessBooking.Application;

public sealed class RefundPolicy
{
    // Simple rule set (change later if you want):
    // >= 24h before: 100%
    // 2h..24h: 50%
    // < 2h: 0%
    public decimal GetRefundAmount(decimal pricePaid, DateTime classStartUtc, DateTime cancelUtc)
    {
        if (pricePaid < 0) throw new ArgumentOutOfRangeException(nameof(pricePaid));
        if (cancelUtc > classStartUtc) return 0m; // cancelling after start => 0

        var delta = classStartUtc - cancelUtc;

        if (delta >= TimeSpan.FromHours(24)) return pricePaid;
        if (delta >= TimeSpan.FromHours(2)) return pricePaid * 0.5m;
        return 0m;
    }
}
