using System;
using System.Threading.Tasks;
using FitnessBooking.Domain;
using FitnessBooking.Infrastructure;
using NUnit.Framework;

namespace FitnessBooking.UnitTests;

public class InMemoryReservationRepositoryTests
{
    [Test]
    public async Task CountActiveForClassAsync_counts_only_active_for_target_class()
    {
        var repo = new InMemoryReservationRepository();

        var classA = Guid.NewGuid();
        var classB = Guid.NewGuid();

        // A + active  => should count
        await repo.AddAsync(new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            ClassId = classA,
            ReservedAtUtc = DateTime.UtcNow,
            PricePaid = 10m,
            CancelledAtUtc = null
        });

        // A + cancelled => should NOT count (covers: ClassId true, CancelledAtUtc false)
        await repo.AddAsync(new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            ClassId = classA,
            ReservedAtUtc = DateTime.UtcNow,
            PricePaid = 10m,
            CancelledAtUtc = DateTime.UtcNow
        });

        // B + active => should NOT count (covers: ClassId false -> short-circuit)
        await repo.AddAsync(new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            ClassId = classB,
            ReservedAtUtc = DateTime.UtcNow,
            PricePaid = 10m,
            CancelledAtUtc = null
        });

        var countA = await repo.CountActiveForClassAsync(classA);
        Assert.That(countA, Is.EqualTo(1));
    }

    [Test]
    public async Task ExistsActiveAsync_true_only_for_exact_active_match()
    {
        var repo = new InMemoryReservationRepository();

        var member = Guid.NewGuid();
        var classId = Guid.NewGuid();

        // Exact active match => true (covers all conditions true)
        await repo.AddAsync(new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = member,
            ClassId = classId,
            ReservedAtUtc = DateTime.UtcNow,
            PricePaid = 10m,
            CancelledAtUtc = null
        });

        var exists = await repo.ExistsActiveAsync(member, classId);
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsActiveAsync_false_when_only_cancelled_match_exists()
    {
        var repo = new InMemoryReservationRepository();

        var member = Guid.NewGuid();
        var classId = Guid.NewGuid();

        // Member true, Class true, CancelledAtUtc NOT null => should be false
        await repo.AddAsync(new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = member,
            ClassId = classId,
            ReservedAtUtc = DateTime.UtcNow,
            PricePaid = 10m,
            CancelledAtUtc = DateTime.UtcNow
        });

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

        // Member true, Class false => should short-circuit => false
        await repo.AddAsync(new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = member,
            ClassId = otherClass,
            ReservedAtUtc = DateTime.UtcNow,
            PricePaid = 10m,
            CancelledAtUtc = null
        });

        var exists = await repo.ExistsActiveAsync(member, classWanted);
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task GetAsync_returns_null_when_not_found_and_reservation_when_found()
    {
        var repo = new InMemoryReservationRepository();

        var id = Guid.NewGuid();

        var missing = await repo.GetAsync(id);
        Assert.That(missing, Is.Null);

        await repo.AddAsync(new Reservation
        {
            Id = id,
            MemberId = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            ReservedAtUtc = DateTime.UtcNow,
            PricePaid = 10m,
            CancelledAtUtc = null
        });

        var found = await repo.GetAsync(id);
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.Id, Is.EqualTo(id));
    }
}
