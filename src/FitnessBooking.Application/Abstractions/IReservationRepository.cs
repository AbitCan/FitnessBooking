using FitnessBooking.Domain;

namespace FitnessBooking.Application.Abstractions;

public interface IReservationRepository
{
    Task<int> CountActiveForClassAsync(Guid classId, CancellationToken ct = default);
    Task<bool> ExistsActiveAsync(Guid memberId, Guid classId, CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);

    Task<Reservation?> GetAsync(Guid reservationId, CancellationToken ct = default);
    Task UpdateAsync(Reservation reservation, CancellationToken ct = default);
}
