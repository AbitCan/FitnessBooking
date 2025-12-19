using FitnessBooking.Application;
using FitnessBooking.Application.Abstractions;
using FitnessBooking.Domain;
using Moq;
using NUnit.Framework;

namespace FitnessBooking.UnitTests;

public class ReservationServiceGuardTests
{
    [Test]
    public async Task Returns_MemberNotFound_when_member_is_missing()
    {
        var members = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classes = new Mock<IClassRepository>(MockBehavior.Strict);
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);

        members.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Member?)null);

        var svc = new ReservationService(members.Object, classes.Object, reservations.Object);

        var result = await svc.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo(CreateReservationError.MemberNotFound));

        // Important: ensure we stop early (kills “keep going anyway” mutants)
        classes.Verify(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        reservations.Verify(r => r.ExistsActiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        reservations.Verify(r => r.CountActiveForClassAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        reservations.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Returns_ClassNotFound_when_class_is_missing()
    {
        var members = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classes = new Mock<IClassRepository>(MockBehavior.Strict);
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);

        var member = new Member { Name = "M", MembershipType = MembershipType.Standard };

        members.Setup(r => r.GetAsync(member.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(member);

        classes.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((FitnessClass?)null);

        var svc = new ReservationService(members.Object, classes.Object, reservations.Object);

        var result = await svc.CreateAsync(member.Id, Guid.NewGuid(), DateTime.UtcNow);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo(CreateReservationError.ClassNotFound));

        reservations.Verify(r => r.ExistsActiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        reservations.Verify(r => r.CountActiveForClassAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        reservations.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Returns_ClassFull_when_capacity_is_zero_or_negative()
    {
        var members = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classes = new Mock<IClassRepository>(MockBehavior.Strict);
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);

        var member = new Member { Name = "M", MembershipType = MembershipType.Standard };
        var cls = new FitnessClass
        {
            Name = "Yoga",
            Instructor = "I",
            Capacity = 0,
            StartAtUtc = DateTime.UtcNow.AddDays(1)
        };

        members.Setup(r => r.GetAsync(member.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(member);

        classes.Setup(r => r.GetAsync(cls.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(cls);

        var svc = new ReservationService(members.Object, classes.Object, reservations.Object);

        var result = await svc.CreateAsync(member.Id, cls.Id, DateTime.UtcNow);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo(CreateReservationError.ClassFull));

        reservations.Verify(r => r.ExistsActiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        reservations.Verify(r => r.CountActiveForClassAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        reservations.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Returns_DuplicateReservation_when_active_reservation_exists()
    {
        var members = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classes = new Mock<IClassRepository>(MockBehavior.Strict);
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);

        var member = new Member { Name = "M", MembershipType = MembershipType.Standard };
        var cls = new FitnessClass
        {
            Name = "Yoga",
            Instructor = "I",
            Capacity = 10,
            StartAtUtc = DateTime.UtcNow.AddDays(1)
        };

        members.Setup(r => r.GetAsync(member.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(member);

        classes.Setup(r => r.GetAsync(cls.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(cls);

        reservations.Setup(r => r.ExistsActiveAsync(member.Id, cls.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        var svc = new ReservationService(members.Object, classes.Object, reservations.Object);

        var result = await svc.CreateAsync(member.Id, cls.Id, DateTime.UtcNow);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo(CreateReservationError.DuplicateReservation));

        // Should not proceed to capacity count or add
        reservations.Verify(r => r.CountActiveForClassAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        reservations.Verify(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
