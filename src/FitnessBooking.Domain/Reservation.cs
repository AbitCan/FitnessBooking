namespace FitnessBooking.Domain;

public sealed class Reservation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid MemberId { get; init; }
    public Guid ClassId { get; init; }
    public decimal PricePaid { get; init; }
    public DateTime ReservedAtUtc { get; init; }
    public DateTime? CancelledAtUtc { get; set; }
}
