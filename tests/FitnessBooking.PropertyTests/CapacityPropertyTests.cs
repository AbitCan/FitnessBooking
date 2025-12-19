using FitnessBooking.Application;
using FitnessBooking.Application.Abstractions;
using FitnessBooking.Domain;
using Moq;

namespace FitnessBooking.PropertyTests;

#pragma warning disable NUnit1027 // FsCheck supplies the parameters, NUnit analyzer doesn't know that
public class CapacityPropertyTests
{
    [FsCheck.NUnit.Property(MaxTest = 200)]
    public bool SuccessfulReservationsNeverExceedCapacity(int capacityRaw, int attemptsRaw)
    {
        var capacity = Math.Clamp(Math.Abs(capacityRaw), 1, 10);
        var attempts = Math.Clamp(Math.Abs(attemptsRaw), 1, 50);

        var membersRepo = new Mock<IMemberRepository>(MockBehavior.Loose);
        var classesRepo = new Mock<IClassRepository>(MockBehavior.Loose);
        var reservationsRepo = new Mock<IReservationRepository>(MockBehavior.Loose);

        var cls = new FitnessClass
        {
            Name = "Any",
            Instructor = "Any",
            Capacity = capacity,
            StartAtUtc = DateTime.UtcNow.AddDays(1)
        };

        classesRepo.Setup(r => r.GetAsync(cls.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(cls);

        membersRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Guid id, CancellationToken _) => new Member
                   {
                       Id = id,
                       Name = "M",
                       MembershipType = MembershipType.Standard
                   });

        var activeCount = 0;

        reservationsRepo.Setup(r => r.ExistsActiveAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

        reservationsRepo.Setup(r => r.CountActiveForClassAsync(cls.Id, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() => activeCount);

        reservationsRepo.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
                        .Callback(() => activeCount++)
                        .Returns(Task.CompletedTask);

        var service = new ReservationService(membersRepo.Object, classesRepo.Object, reservationsRepo.Object);

        var success = 0;
        for (int i = 0; i < attempts; i++)
        {
            var r = service.CreateAsync(Guid.NewGuid(), cls.Id, DateTime.UtcNow).GetAwaiter().GetResult();
            if (r.Success) success++;
        }

        return success <= capacity;
    }
}
#pragma warning restore NUnit1027
