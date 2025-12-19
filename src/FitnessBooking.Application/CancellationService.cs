using FitnessBooking.Application.Abstractions;

namespace FitnessBooking.Application;

public enum CancelReservationError
{
    None = 0,
    ReservationNotFound,
    AlreadyCancelled,
    ClassNotFound
}

public sealed record CancelReservationResult(bool Success, decimal Refund, CancelReservationError Error);

public sealed class CancellationService
{
    private readonly IReservationRepository _reservations;
    private readonly IClassRepository _classes;
    private readonly RefundPolicy _refundPolicy;

    public CancellationService(IReservationRepository reservations, IClassRepository classes, RefundPolicy refundPolicy)
    {
        _reservations = reservations;
        _classes = classes;
        _refundPolicy = refundPolicy;
    }

    public async Task<CancelReservationResult> CancelAsync(Guid reservationId, DateTime cancelUtc, CancellationToken ct = default)
    {
        var res = await _reservations.GetAsync(reservationId, ct);
        if (res is null)
            return new(false, 0m, CancelReservationError.ReservationNotFound);

        if (res.CancelledAtUtc is not null)
            return new(false, 0m, CancelReservationError.AlreadyCancelled);

        var cls = await _classes.GetAsync(res.ClassId, ct);
        if (cls is null)
            return new(false, 0m, CancelReservationError.ClassNotFound);

        var refund = _refundPolicy.GetRefundAmount(res.PricePaid, cls.StartAtUtc, cancelUtc);

        res.CancelledAtUtc = cancelUtc;
        await _reservations.UpdateAsync(res, ct);

        return new(true, refund, CancelReservationError.None);
    }
}
