using FitnessBooking.Domain;

namespace FitnessBooking.Application.Abstractions;

public interface IMemberRepository
{
    Task<Member?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Member member, CancellationToken ct = default);
}
