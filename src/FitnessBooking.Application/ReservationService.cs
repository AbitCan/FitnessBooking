using FitnessBooking.Application.Abstractions;
using FitnessBooking.Domain;

namespace FitnessBooking.Application;

public enum CreateReservationError
{
    None = 0,
    MemberNotFound,
    ClassNotFound,
    ClassFull,
    DuplicateReservation
}

public sealed record CreateReservationResult(
    bool Success,
    Guid? ReservationId,
    CreateReservationError Error);

public sealed class ReservationService
{
    private readonly IMemberRepository _members;
    private readonly IClassRepository _classes;
    private readonly IReservationRepository _reservations;

    public ReservationService(
        IMemberRepository members,
        IClassRepository classes,
        IReservationRepository reservations)
    {
        _members = members;
        _classes = classes;
        _reservations = reservations;
    }

    public async Task<CreateReservationResult> CreateAsync(
    Guid memberId,
    Guid classId,
    DateTime nowUtc,
    CancellationToken ct = default)
    {
        var member = await _members.GetAsync(memberId, ct);
        if (member is null)
            return new(false, null, CreateReservationError.MemberNotFound);

        var fitnessClass = await _classes.GetAsync(classId, ct);
        if (fitnessClass is null)
            return new(false, null, CreateReservationError.ClassNotFound);

        if (fitnessClass.Capacity <= 0)
            return new(false, null, CreateReservationError.ClassFull);

        // Prevent duplicate active reservation for same member+class
        if (await _reservations.ExistsActiveAsync(memberId, classId, ct))
            return new(false, null, CreateReservationError.DuplicateReservation);

        // Capacity invariant
        var activeCount = await _reservations.CountActiveForClassAsync(classId, ct);
        if (activeCount >= fitnessClass.Capacity)
            return new(false, null, CreateReservationError.ClassFull);

        // ---- Pricing (simple dynamic pricing) ----
        const decimal basePrice = 100m;

        // Occupancy ratio based on current active reservations
        var occupancyRatio = (decimal)activeCount / fitnessClass.Capacity;

        // Occupancy multiplier: Low/Mid/High
        var occupancyMultiplier =
            occupancyRatio >= 0.80m ? 1.30m :
            occupancyRatio >= 0.40m ? 1.10m :
                                      1.00m;

        // Peak vs OffPeak (example rule: 17:00–21:59 UTC is peak)
        var hour = fitnessClass.StartAtUtc.Hour;
        var isPeak = hour >= 17 && hour < 22;
        var timeMultiplier = isPeak ? 1.20m : 1.00m;

        // Membership multiplier (discounts)
        var membershipMultiplier = member.MembershipType switch
        {
            MembershipType.Premium => 0.80m,
            MembershipType.Student => 0.70m,
            _ => 1.00m
        };

        var price = decimal.Round(basePrice * occupancyMultiplier * timeMultiplier * membershipMultiplier, 2);
        // -----------------------------------------

        var reservation = new Reservation
        {
            MemberId = memberId,
            ClassId = classId,
            ReservedAtUtc = nowUtc,
            PricePaid = price
        };

        await _reservations.AddAsync(reservation, ct);
        return new(true, reservation.Id, CreateReservationError.None);
    }

}
