using System;
using System.Threading.Tasks;
using FitnessBooking.Domain;
using FitnessBooking.Infrastructure;
using NUnit.Framework;

namespace FitnessBooking.UnitTests;

public class InMemoryReservationRepositoryTests
{
    private static Reservation MakeReservation(
        Guid id,
        Guid memberId,
        Guid classId,
        DateTime? cancelledAtUtc)
    {
        return new Reservation
        {
            Id = id,
            MemberId = memberId,
            ClassId = classId,
            ReservedAtUtc = DateTime.UtcNow,
            PricePaid = 10m,
            CancelledAtUtc = cancelledAtUtc
        };
    }

    [Test]
    public async Task CountActiveForClassAsync_counts_only_active_for_target_class()
    {
        var repo = new InMemoryReservationRepository();

        var classA = Guid.NewGuid();
        var classB = Guid.NewGuid();

        // (true && true) => count
        await repo.AddAsync(MakeReservation(Guid.NewGuid(), Guid.NewGuid(), classA, cancelledAtUtc: null));

        // (true && false) => do NOT count
        await repo.AddAsync(MakeReservation(Guid.NewGuid(), Guid.NewGuid(), classA, cancelledAtUtc: DateTime.UtcNow));

        // (false && ...) short-circuit => do NOT count
        await repo.AddAsync(MakeReservation(Guid.NewGuid(), Guid.NewGuid(), classB, cancelledAtUtc: null));

        var countA = await repo.CountActiveForClassAsync(classA);

        Assert.That(countA, Is.EqualTo(1));
    }

    [Test]
    public async Task ExistsActiveAsync_true_only_for_exact_active_match()
    {
        var repo = new InMemoryReservationRepository();

        var member = Guid.NewGuid();
        var classId = Guid.NewGuid();

        await repo.AddAsync(MakeReservation(Guid.NewGuid(), member, classId, cancelledAtUtc: null));

        var exists = await repo.ExistsActiveAsync(member, classId);

        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsActiveAsync_false_when_only_cancelled_match_exists()
    {
        var repo = new InMemoryReservationRepository();

        var member = Guid.NewGuid();
        var classId = Guid.NewGuid();

        // member true, class true, cancelled false => overall false
        await repo.AddAsync(MakeReservation(Guid.NewGuid(), member, classId, cancelledAtUtc: DateTime.UtcNow));

        var exists = await repo.ExistsActiveAsync(member, classId);

        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task ExistsActiveAsync_false_when_member_matches_but_class_differs()
    {
        var repo = new InMemoryReservationRepository();

        var member = Guid.NewGuid();
        var classWanted = Guid.NewGuid();
        var otherClass = Guid.NewGuid();

        // member true, class false => short-circuit => false
        await repo.AddAsync(MakeReservation(Guid.NewGuid(), member, otherClass, cancelledAtUtc: null));

        var exists = await repo.ExistsActiveAsync(member, classWanted);

        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task ExistsActiveAsync_false_when_memberId_does_not_match()
    {
        var repo = new InMemoryReservationRepository();

        var realMember = Guid.NewGuid();
        var otherMember = Guid.NewGuid();
        var classId = Guid.NewGuid();

        // member false => short-circuit => false
        await repo.AddAsync(MakeReservation(Guid.NewGuid(), realMember, classId, cancelledAtUtc: null));

        var exists = await repo.ExistsActiveAsync(otherMember, classId);

        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task GetAsync_returns_null_when_not_found_and_reservation_when_found()
    {
        var repo = new InMemoryReservationRepository();

        var id = Guid.NewGuid();

        var missing = await repo.GetAsync(id);
        Assert.That(missing, Is.Null);

        await repo.AddAsync(MakeReservation(id, Guid.NewGuid(), Guid.NewGuid(), cancelledAtUtc: null));

        var found = await repo.GetAsync(id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(id));
    }
}
