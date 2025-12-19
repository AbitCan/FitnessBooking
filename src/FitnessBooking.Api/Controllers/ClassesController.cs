using FitnessBooking.Application.Abstractions;
using FitnessBooking.Domain;
using Microsoft.AspNetCore.Mvc;

namespace FitnessBooking.Api.Controllers;

[ApiController]
[Route("classes")]
public sealed class ClassesController : ControllerBase
{
    private readonly IClassRepository _classes;

    public ClassesController(IClassRepository classes) => _classes = classes;

    public sealed record CreateClassRequest(string Name, string Instructor, int Capacity, DateTime StartAtUtc);
    public sealed record CreateClassResponse(Guid Id);

    [HttpPost]
    public async Task<ActionResult<CreateClassResponse>> Create(CreateClassRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Instructor))
            return BadRequest("Name and Instructor are required.");
        if (req.Capacity <= 0)
            return BadRequest("Capacity must be > 0.");
        if (req.StartAtUtc.Kind != DateTimeKind.Utc)
            return BadRequest("StartAtUtc must be UTC.");

        var fc = new FitnessClass
        {
            Name = req.Name.Trim(),
            Instructor = req.Instructor.Trim(),
            Capacity = req.Capacity,
            StartAtUtc = req.StartAtUtc
        };

        await _classes.AddAsync(fc, ct);
        return Created($"/classes/{fc.Id}", new CreateClassResponse(fc.Id));
    }
}
