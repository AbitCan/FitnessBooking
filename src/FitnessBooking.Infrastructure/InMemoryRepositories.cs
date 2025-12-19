using System.Collections.Concurrent;
using FitnessBooking.Application.Abstractions;
using FitnessBooking.Domain;
using System.Linq;

namespace FitnessBooking.Infrastructure;

public sealed class InMemoryMemberRepository : IMemberRepository
{
    private readonly ConcurrentDictionary<Guid, Member> _members = new();

    public Task<Member?> GetAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_members.TryGetValue(id, out var m) ? m : null);

    public Task AddAsync(Member member, CancellationToken ct = default)
    {
        _members[member.Id] = member;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryClassRepository : IClassRepository
{
    private readonly ConcurrentDictionary<Guid, FitnessClass> _classes = new();

    public Task<FitnessClass?> GetAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_classes.TryGetValue(id, out var c) ? c : null);

    public Task AddAsync(FitnessClass fitnessClass, CancellationToken ct = default)
    {
        _classes[fitnessClass.Id] = fitnessClass;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly object _lock = new();
    private readonly List<Reservation> _reservations = new();

    public Task<int> CountActiveForClassAsync(Guid classId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var count = _reservations.Count(r => r.ClassId == classId && r.CancelledAtUtc is null);
            return Task.FromResult(count);
        }
    }

    public Task<bool> ExistsActiveAsync(Guid memberId, Guid classId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var exists = _reservations.Any(r =>
                r.MemberId == memberId &&
                r.ClassId == classId &&
                r.CancelledAtUtc is null);

            return Task.FromResult(exists);
        }
    }

    public Task AddAsync(Reservation reservation, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _reservations.Add(reservation);
            return Task.CompletedTask;
        }
    }
    public Task<Reservation?> GetAsync(Guid reservationId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var res = _reservations.FirstOrDefault(r => r.Id == reservationId);
            return Task.FromResult(res);
        }
    }

    public Task UpdateAsync(Reservation reservation, CancellationToken ct = default)
    {
        // For in-memory list of objects, the object reference is already updated.
        // This method exists for future DB implementation.
        return Task.CompletedTask;
    }

}
