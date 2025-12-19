using FitnessBooking.Application.Abstractions;
using FitnessBooking.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FitnessBooking.Api.Controllers;

[ApiController]
[Route("members")]
public sealed class MembersController : ControllerBase
{
    private readonly IMemberRepository _members;

    public MembersController(IMemberRepository members) => _members = members;

    public sealed record CreateMemberRequest(string Name, MembershipType MembershipType);
    public sealed record CreateMemberResponse(Guid Id);

    [HttpPost]
    public async Task<ActionResult<CreateMemberResponse>> Create(CreateMemberRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Name is required.");

        var member = new Member { Name = req.Name.Trim(), MembershipType = req.MembershipType };
        await _members.AddAsync(member, ct);

        return Created($"/members/{member.Id}", new CreateMemberResponse(member.Id));
    }
}
