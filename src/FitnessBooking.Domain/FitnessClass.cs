namespace FitnessBooking.Domain;

public sealed class FitnessClass
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required string Instructor { get; init; }
    public int Capacity { get; init; }
    public DateTime StartAtUtc { get; init; }
}
