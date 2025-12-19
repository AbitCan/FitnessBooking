using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace FitnessBooking.IntegrationTests;

public class CancelReservationApiTests
{
    [Test]
    public async Task Canceling_a_reservation_frees_capacity()
    {
        using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var m1 = await (await client.PostAsJsonAsync("/members", new { name = "A", membershipType = 0 }))
            .Content.ReadFromJsonAsync<IdResponse>();
        var m2 = await (await client.PostAsJsonAsync("/members", new { name = "B", membershipType = 0 }))
            .Content.ReadFromJsonAsync<IdResponse>();

        var startUtc = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1), DateTimeKind.Utc);
        var cls = await (await client.PostAsJsonAsync("/classes", new { name = "Yoga", instructor = "I1", capacity = 1, startAtUtc = startUtc }))
            .Content.ReadFromJsonAsync<IdResponse>();

        var r1 = await (await client.PostAsJsonAsync("/reservations", new { memberId = m1!.Id, classId = cls!.Id }))
            .Content.ReadFromJsonAsync<IdResponse>();

        var cancel = await client.DeleteAsync($"/reservations/{r1!.Id}");
        Assert.That(cancel.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var r2 = await client.PostAsJsonAsync("/reservations", new { memberId = m2!.Id, classId = cls!.Id });
        Assert.That(r2.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    private sealed record IdResponse(Guid Id);
}
