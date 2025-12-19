using FitnessBooking.Application;
using FitnessBooking.Application.Abstractions;
using FitnessBooking.Domain;
using Moq;
using NUnit.Framework;

namespace FitnessBooking.UnitTests;

public class CancellationServiceTests
{
    [Test]
    public async Task Cancel_returns_not_found_when_reservation_missing()
    {
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);
        var classes = new Mock<IClassRepository>(MockBehavior.Strict);
        var policy = new RefundPolicy();

        reservations.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Reservation?)null);

        var svc = new CancellationService(reservations.Object, classes.Object, policy);

        var result = await svc.CancelAsync(Guid.NewGuid(), DateTime.UtcNow);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo(CancelReservationError.ReservationNotFound));
        Assert.That(result.Refund, Is.EqualTo(0m));
    }

    [Test]
    public async Task Cancel_returns_conflict_when_already_cancelled()
    {
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);
        var classes = new Mock<IClassRepository>(MockBehavior.Strict);
        var policy = new RefundPolicy();

        var res = new Reservation
        {
            MemberId = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            PricePaid = 200m,
            CancelledAtUtc = DateTime.UtcNow.AddMinutes(-1)
        };

        reservations.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(res);

        var svc = new CancellationService(reservations.Object, classes.Object, policy);

        var result = await svc.CancelAsync(Guid.NewGuid(), DateTime.UtcNow);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo(CancelReservationError.AlreadyCancelled));
        Assert.That(result.Refund, Is.EqualTo(0m));
    }

    [Test]
    public async Task Cancel_returns_class_not_found_when_class_missing()
    {
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);
        var classes = new Mock<IClassRepository>(MockBehavior.Strict);
        var policy = new RefundPolicy();

        var res = new Reservation
        {
            MemberId = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            PricePaid = 200m
        };

        reservations.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(res);

        classes.Setup(r => r.GetAsync(res.ClassId, It.IsAny<CancellationToken>()))
               .ReturnsAsync((FitnessClass?)null);

        var svc = new CancellationService(reservations.Object, classes.Object, policy);

        var result = await svc.CancelAsync(Guid.NewGuid(), DateTime.UtcNow);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.EqualTo(CancelReservationError.ClassNotFound));
        Assert.That(result.Refund, Is.EqualTo(0m));
    }

    [Test]
    public async Task Cancel_happy_path_sets_cancelled_time_updates_and_refunds()
    {
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);
        var classes = new Mock<IClassRepository>(MockBehavior.Strict);
        var policy = new RefundPolicy();

        var startUtc = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var cancelUtc = startUtc.AddHours(-24); // GE24H => full refund per your rules

        var res = new Reservation
        {
            Id = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            ClassId = Guid.NewGuid(),
            PricePaid = 200m,
            CancelledAtUtc = null
        };

        var cls = new FitnessClass { Id = res.ClassId, StartAtUtc = startUtc, Capacity = 10, Name = "X", Instructor = "Y" };

        reservations.Setup(r => r.GetAsync(res.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(res);

        classes.Setup(r => r.GetAsync(res.ClassId, It.IsAny<CancellationToken>()))
               .ReturnsAsync(cls);

        reservations.Setup(r => r.UpdateAsync(res, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var svc = new CancellationService(reservations.Object, classes.Object, policy);

        var result = await svc.CancelAsync(res.Id, cancelUtc);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Error, Is.EqualTo(CancelReservationError.None));
        Assert.That(result.Refund, Is.EqualTo(200m));
        Assert.That(res.CancelledAtUtc, Is.EqualTo(cancelUtc));

        reservations.Verify(r => r.UpdateAsync(res, It.IsAny<CancellationToken>()), Times.Once);
    }
}
