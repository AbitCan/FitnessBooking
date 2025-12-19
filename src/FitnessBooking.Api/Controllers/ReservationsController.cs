using FitnessBooking.Application;
using Microsoft.AspNetCore.Mvc;

namespace FitnessBooking.Api.Controllers;

[ApiController]
[Route("reservations")]
public sealed class ReservationsController : ControllerBase
{
    private readonly ReservationService _service;
    private readonly CancellationService _cancellation;
    public ReservationsController(ReservationService service, CancellationService cancellation)
    {
        _service = service;
        _cancellation = cancellation;
    }

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
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _cancellation.CancelAsync(id, DateTime.UtcNow, ct);

        if (result.Success)
            return Ok(new { refund = result.Refund });

        return result.Error switch
        {
            CancelReservationError.ReservationNotFound => NotFound("Reservation not found."),
            CancelReservationError.ClassNotFound => NotFound("Class not found."),
            CancelReservationError.AlreadyCancelled => Conflict("Already cancelled."),
            _ => StatusCode(500, "Unknown error.")
        };
    }

}
