using FitnessBooking.Application;
using FitnessBooking.Application.Abstractions;
using FitnessBooking.Domain;
using FluentAssertions;
using Moq;

namespace FitnessBooking.UnitTests;

public class ReservationServiceTests
{
    [Test]
    public async Task When_class_is_full_second_reservation_is_rejected()
    {
        // Arrange
        var membersRepo = new Mock<IMemberRepository>(MockBehavior.Strict);
        var classesRepo = new Mock<IClassRepository>(MockBehavior.Strict);
        var reservationsRepo = new Mock<IReservationRepository>(MockBehavior.Strict);

        var m1 = new Member { Name = "A", MembershipType = MembershipType.Standard };
        var m2 = new Member { Name = "B", MembershipType = MembershipType.Standard };

        var fc = new FitnessClass
        {
            Name = "Yoga",
            Instructor = "I1",
            Capacity = 1,
            StartAtUtc = DateTime.UtcNow.AddDays(1)
        };

        membersRepo.Setup(r => r.GetAsync(m1.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(m1);
        membersRepo.Setup(r => r.GetAsync(m2.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(m2);

        classesRepo.Setup(r => r.GetAsync(fc.Id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(fc);

        // In-memory "state" inside the mock so CountActive reflects AddAsync
        var active = new HashSet<(Guid MemberId, Guid ClassId)>();

        reservationsRepo
            .Setup(r => r.ExistsActiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid memberId, Guid classId, CancellationToken _) => active.Contains((memberId, classId)));

        reservationsRepo
            .Setup(r => r.CountActiveForClassAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid classId, CancellationToken _) => active.Count(x => x.ClassId == classId));

        reservationsRepo
            .Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .Callback((Reservation res, CancellationToken _) => active.Add((res.MemberId, res.ClassId)))
            .Returns(Task.CompletedTask);

        var service = new ReservationService(membersRepo.Object, classesRepo.Object, reservationsRepo.Object);

        // Act
        var r1 = await service.CreateAsync(m1.Id, fc.Id, DateTime.UtcNow);
        var r2 = await service.CreateAsync(m2.Id, fc.Id, DateTime.UtcNow);

        // Assert
        r1.Success.Should().BeTrue();
        r2.Success.Should().BeFalse();
        r2.Error.Should().Be(CreateReservationError.ClassFull);
    }
}
