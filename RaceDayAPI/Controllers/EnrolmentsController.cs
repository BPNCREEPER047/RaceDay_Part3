using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Middleware;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/enrolments")]
    [Produces("application/json")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EnrolmentsController(AppDbContext db) => _db = db;

        /// <summary>Returns all enrolments for the logged-in Participant.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EnrolmentResponseDto>), 200)]
        [ProducesResponseType(401)][ProducesResponseType(403)]
        public async Task<IActionResult> GetMyEnrolments()
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });
            if (!HttpContext.Session.IsParticipant())
                return StatusCode(403, new { message = "Only Participants can view their enrolments." });

            var userId = HttpContext.Session.GetUserId()!.Value;

            var enrolments = await _db.Enrolments
                .Include(en => en.Event)
                    .ThenInclude(ev => ev!.Category)
                .Include(en => en.Participant)
                .Where(en => en.ParticipantID == userId)
                .OrderByDescending(en => en.EnrolledAt)
                .ToListAsync();

            return Ok(enrolments.Select(MapToDto));
        }

        /// <summary>Enrols the logged-in Participant into an event.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(EnrolmentResponseDto), 201)]
        [ProducesResponseType(400)][ProducesResponseType(401)][ProducesResponseType(403)]
        [ProducesResponseType(404)][ProducesResponseType(409)]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });
            if (!HttpContext.Session.IsParticipant())
                return StatusCode(403, new { message = "Only Participants can enrol in events." });
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.Session.GetUserId()!.Value;

            var ev = await _db.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EventID == dto.EventID);

            if (ev == null)
                return NotFound(new { message = $"Event {dto.EventID} not found." });

            var alreadyEnrolled = await _db.Enrolments
                .AnyAsync(en => en.ParticipantID == userId && en.EventID == dto.EventID);
            if (alreadyEnrolled)
                return Conflict(new { message = "You are already enrolled in this event." });

            var currentCount = await _db.Enrolments
                .CountAsync(en => en.EventID == dto.EventID && en.Status == "Registered");
            if (currentCount >= ev.MaxParticipants)
                return Conflict(new { message = "This event has reached its maximum capacity." });

            var enrolment = new Enrolment
            {
                ParticipantID = userId,
                EventID       = dto.EventID,
                EnrolledAt    = DateTime.UtcNow,
                Status        = "Registered"
            };

            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            await _db.Entry(enrolment).Reference(en => en.Participant).LoadAsync();
            enrolment.Event = ev; // already loaded

            return CreatedAtAction(nameof(GetMyEnrolments), MapToDto(enrolment));
        }

        /// <summary>Cancels a specific enrolment. Participant only (own enrolments).</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(200)][ProducesResponseType(401)][ProducesResponseType(403)][ProducesResponseType(404)]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });
            if (!HttpContext.Session.IsParticipant())
                return StatusCode(403, new { message = "Only Participants can cancel enrolments." });

            var userId    = HttpContext.Session.GetUserId()!.Value;
            var enrolment = await _db.Enrolments.FindAsync(id);

            if (enrolment == null)
                return NotFound(new { message = $"Enrolment {id} not found." });
            if (enrolment.ParticipantID != userId)
                return StatusCode(403, new { message = "You can only cancel your own enrolments." });

            _db.Enrolments.Remove(enrolment);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Enrolment cancelled." });
        }

        // ── helper ───────────────────────────────────────────────────────────
        private static EnrolmentResponseDto MapToDto(Enrolment en) => new()
        {
            EnrolmentID     = en.EnrolmentID,
            ParticipantID   = en.ParticipantID,
            ParticipantName = en.Participant?.FullName ?? string.Empty,
            EventID         = en.EventID,
            EventName       = en.Event?.EventName ?? string.Empty,
            CategoryName    = en.Event?.Category?.CategoryName ?? string.Empty,
            EnrolledAt      = en.EnrolledAt,
            Status          = en.Status
        };
    }
}
