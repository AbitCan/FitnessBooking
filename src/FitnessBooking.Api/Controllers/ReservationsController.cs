using FitnessBooking.Application;
using Microsoft.AspNetCore.Mvc;

namespace FitnessBooking.Api.Controllers;

[ApiController]
[Route("reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly ReservationService _service;

    public ReservationsController(ReservationService service) => _service = service;

    public sealed record CreateReservationRequest(Guid MemberId, Guid ClassId);
    public sealed record CreateReservationResponse(Guid Id);

    [HttpPost]
    public async Task<ActionResult<CreateReservationResponse>> Create(CreateReservationRequest req, CancellationToken ct)
    {
        if (req.MemberId == Guid.Empty || req.ClassId == Guid.Empty)
            return BadRequest("MemberId and ClassId are required.");

        var result = await _service.CreateAsync(req.MemberId, req.ClassId, DateTime.UtcNow, ct);

        if (result.Success)
            return Created($"/reservations/{result.ReservationId}", new CreateReservationResponse(result.ReservationId!.Value));

        return result.Error switch
        {
            CreateReservationError.MemberNotFound => NotFound("Member not found."),
            CreateReservationError.ClassNotFound => NotFound("Class not found."),
            CreateReservationError.ClassFull => Conflict("Class is full."),
            CreateReservationError.DuplicateReservation => Conflict("Duplicate reservation."),
            _ => StatusCode(500, "Unknown error.")
        };
    }
}
