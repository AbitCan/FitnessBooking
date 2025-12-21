using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace FitnessBooking.IntegrationTests;

public class ApiControllerBranchTests
{
    private WebApplicationFactory<global::Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<global::Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ---- DTOs that match your API ----
    private sealed record IdResponse(Guid Id);

    // If your member endpoint requires more fields, add them here.
    private sealed record CreateMemberRequest(string Name);

    private sealed record CreateClassRequest(string Name, string Instructor, int Capacity, DateTime StartAtUtc);

    private sealed record CreateReservationRequest(Guid MemberId, Guid ClassId);

    // ---- Helpers ----
    private async Task<Guid> CreateMemberAsync(string name = "Test Member")
    {
        var res = await _client.PostAsJsonAsync("/members", new CreateMemberRequest(name));
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var body = await res.Content.ReadFromJsonAsync<IdResponse>();
        Assert.That(body, Is.Not.Null);
        return body!.Id;
    }

    private async Task<Guid> CreateClassAsync(DateTime startAtUtc, int capacity = 10)
    {
        var req = new CreateClassRequest("Test Class", "Coach", capacity, startAtUtc);
        var res = await _client.PostAsJsonAsync("/classes", req);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var body = await res.Content.ReadFromJsonAsync<IdResponse>();
        Assert.That(body, Is.Not.Null);
        return body!.Id;
    }

    private async Task<Guid> CreateReservationAsync(Guid memberId, Guid classId)
    {
        var res = await _client.PostAsJsonAsync("/reservations", new CreateReservationRequest(memberId, classId));
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var body = await res.Content.ReadFromJsonAsync<IdResponse>();
        Assert.That(body, Is.Not.Null);
        return body!.Id;
    }

    // ---- Tests: Classes ----
    [Test]
    public async Task Post_classes_missing_required_fields_returns_400()
    {
        // missing Name/Instructor should trip model validation
        var res = await _client.PostAsJsonAsync("/classes", new { capacity = 10, startAtUtc = DateTime.UtcNow.AddDays(2) });
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Post_classes_capacity_zero_returns_400()
    {
        var req = new CreateClassRequest("X", "Y", 0, DateTime.UtcNow.AddDays(2));
        var res = await _client.PostAsJsonAsync("/classes", req);
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // ---- Tests: Reservations ----
    [Test]
    public async Task Post_reservations_member_not_found_returns_404()
    {
        var classId = await CreateClassAsync(DateTime.UtcNow.AddDays(2));
        var res = await _client.PostAsJsonAsync("/reservations",
            new CreateReservationRequest(Guid.NewGuid(), classId));
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Post_reservations_class_not_found_returns_404()
    {
        var memberId = await CreateMemberAsync();
        var res = await _client.PostAsJsonAsync("/reservations",
            new CreateReservationRequest(memberId, Guid.NewGuid()));
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Post_reservations_duplicate_returns_409()
    {
        var memberId = await CreateMemberAsync();
        var classId = await CreateClassAsync(DateTime.UtcNow.AddDays(2));

        _ = await CreateReservationAsync(memberId, classId);

        var res2 = await _client.PostAsJsonAsync("/reservations",
            new CreateReservationRequest(memberId, classId));
        Assert.That(res2.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task Post_reservations_class_full_returns_409()
    {
        var classId = await CreateClassAsync(DateTime.UtcNow.AddDays(2), capacity: 1);
        var memberA = await CreateMemberAsync("A");
        var memberB = await CreateMemberAsync("B");

        _ = await CreateReservationAsync(memberA, classId);

        var res2 = await _client.PostAsJsonAsync("/reservations",
            new CreateReservationRequest(memberB, classId));
        Assert.That(res2.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    // ---- Tests: Cancellation ----
    [Test]
    public async Task Delete_reservations_unknown_returns_404()
    {
        var res = await _client.DeleteAsync($"/reservations/{Guid.NewGuid()}");
        Assert.That(res.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_reservations_happy_path_then_second_time_returns_409()
    {
        var memberId = await CreateMemberAsync();
        var classId = await CreateClassAsync(DateTime.UtcNow.AddDays(2));
        var reservationId = await CreateReservationAsync(memberId, classId);

        var cancel1 = await _client.DeleteAsync($"/reservations/{reservationId}");
        Assert.That(cancel1.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var cancel2 = await _client.DeleteAsync($"/reservations/{reservationId}");
        Assert.That(cancel2.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}
