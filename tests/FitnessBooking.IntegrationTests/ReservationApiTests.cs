using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace FitnessBooking.IntegrationTests;

public class ReservationApiTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [SetUp]
    public void Setup() => _factory = new WebApplicationFactory<Program>();

    [TearDown]
    public void TearDown() => _factory.Dispose();

    [Test]
    public async Task Reserving_when_full_returns_409()
    {
        var client = _factory.CreateClient();

        // Create two members
        var m1 = await (await client.PostAsJsonAsync("/members", new { name = "A", membershipType = 0 })).Content.ReadFromJsonAsync<IdResponse>();
        var m2 = await (await client.PostAsJsonAsync("/members", new { name = "B", membershipType = 0 })).Content.ReadFromJsonAsync<IdResponse>();

        // Create class capacity=1
        var startUtc = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(1), DateTimeKind.Utc);
        var cls = await (await client.PostAsJsonAsync("/classes", new { name = "Yoga", instructor = "I1", capacity = 1, startAtUtc = startUtc }))
            .Content.ReadFromJsonAsync<IdResponse>();

        // First reservation ok
        var res1 = await client.PostAsJsonAsync("/reservations", new { memberId = m1!.Id, classId = cls!.Id });
        Assert.That(res1.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        // Second reservation should fail with 409
        var res2 = await client.PostAsJsonAsync("/reservations", new { memberId = m2!.Id, classId = cls!.Id });
        Assert.That(res2.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    private sealed record IdResponse(Guid Id);
}
