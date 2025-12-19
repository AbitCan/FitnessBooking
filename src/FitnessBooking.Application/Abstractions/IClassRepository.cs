using FitnessBooking.Domain;

namespace FitnessBooking.Application.Abstractions;

public interface IClassRepository
{
    Task<FitnessClass?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(FitnessClass fitnessClass, CancellationToken ct = default);
}
