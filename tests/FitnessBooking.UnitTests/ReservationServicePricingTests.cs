using System;
using System.Threading;
using System.Threading.Tasks;
using FitnessBooking.Application;
using FitnessBooking.Application.Abstractions;
using FitnessBooking.Domain;
using Moq;
using NUnit.Framework;

namespace FitnessBooking.UnitTests;

public class ReservationServicePricingTests
{
    [TestCase(MembershipType.Standard, 10, 0, 10, 100.00)] // offpeak, low occupancy, standard
    [TestCase(MembershipType.Premium, 18, 4, 10, 105.60)] // peak, mid occupancy (0.40), premium (0.80)
    [TestCase(MembershipType.Student, 10, 8, 10, 91.00)] // offpeak, high occupancy (0.80), student (0.70)
    public async Task CreateAsync_sets_expected_dynamic_price(
        MembershipType membershipType,
        int classHourUtc,
        int activeCount,
        int capacity,
        double expectedPrice)
    {
        var members = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classes = new Mock<IClassRepository>(MockBehavior.Strict);
        var reservations = new Mock<IReservationRepository>(MockBehavior.Strict);

        var svc = new ReservationService(members.Object, classes.Object, reservations.Object);

        var memberId = Guid.NewGuid();
        var classId = Guid.NewGuid();

        var member = new Member
        {
            Id = memberId,
            Name = "Test Member",
            MembershipType = membershipType
        };

        var startUtc = new DateTime(2030, 1, 1, classHourUtc, 0, 0, DateTimeKind.Utc);
        var fitnessClass = new FitnessClass
        {
            Id = classId,
            Name = "Test",
            Instructor = "X",
            Capacity = capacity,
            StartAtUtc = startUtc
        };

        members.Setup(r => r.GetAsync(memberId, It.IsAny<CancellationToken>()))
               .ReturnsAsync(member);

        classes.Setup(r => r.GetAsync(classId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(fitnessClass);

        reservations.Setup(r => r.ExistsActiveAsync(memberId, classId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        reservations.Setup(r => r.CountActiveForClassAsync(classId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeCount);

        Reservation? captured = null;
        reservations.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
                    .Callback<Reservation, CancellationToken>((res, _) => captured = res)
                    .Returns(Task.CompletedTask);

        var nowUtc = new DateTime(2029, 12, 31, 10, 0, 0, DateTimeKind.Utc);
        var result = await svc.CreateAsync(memberId, classId, nowUtc);

        Assert.That(result.Success, Is.True);
        Assert.That(captured, Is.Not.Null);

        Assert.That(captured!.PricePaid, Is.EqualTo((decimal)expectedPrice));
    }
}
