namespace FitnessBooking.Domain;

public sealed class Member
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public MembershipType MembershipType { get; init; }
}
