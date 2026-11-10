using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Middleware;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/results")]
    [Produces("application/json")]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ResultsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Returns all results for the logged-in Participant's enrolments.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ResultResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetMyResults()
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsParticipant())
                return StatusCode(403, new { message = "Only Participants can view their own results." });

            var userId = HttpContext.Session.GetUserId()!.Value;

            var results = await _db.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Event)
                        .ThenInclude(ev => ev!.Category)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Participant)
                .Where(r => r.Enrolment!.ParticipantID == userId)
                .OrderByDescending(r => r.Enrolment!.Event!.EventDate)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>Returns all results for a specific event. Organiser only.</summary>
        [HttpGet("event/{eventId:int}")]
        [ProducesResponseType(typeof(IEnumerable<ResultResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetResultsByEvent(int eventId)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can view event results." });

            var eventExists = await _db.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists)
                return NotFound(new { message = $"Event with ID {eventId} not found." });

            var results = await _db.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Event)
                        .ThenInclude(ev => ev!.Category)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Participant)
                .Where(r => r.Enrolment!.EventID == eventId)
                .OrderBy(r => r.Position)
                .Select(r => MapToDto(r))
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>Captures a result for a participant's enrolment. Organiser only.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(ResultResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Create([FromBody] CreateResultDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can capture results." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var enrolment = await _db.Enrolments
                .Include(en => en.Event)
                    .ThenInclude(ev => ev!.Category)
                .Include(en => en.Participant)
                .FirstOrDefaultAsync(en => en.EnrolmentID == dto.EnrolmentID);

            if (enrolment == null)
                return NotFound(new { message = $"Enrolment with ID {dto.EnrolmentID} not found." });

            var resultExists = await _db.Results
                .AnyAsync(r => r.EnrolmentID == dto.EnrolmentID);

            if (resultExists)
                return Conflict(new { message = "A result for this enrolment already exists." });

            var result = new Result
            {
                EnrolmentID = dto.EnrolmentID,
                FinishTime  = dto.FinishTime.Trim(),
                Position    = dto.Position,
                Notes       = dto.Notes?.Trim()
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            // Reload for response
            result.Enrolment = enrolment;

            return CreatedAtAction(nameof(GetResultsByEvent),
                new { eventId = enrolment.EventID }, MapToDto(result));
        }

        /// <summary>Updates an existing result. Organiser only.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ResultResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateResultDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can update results." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _db.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Event)
                        .ThenInclude(ev => ev!.Category)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Participant)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
                return NotFound(new { message = $"Result with ID {id} not found." });

            if (!string.IsNullOrWhiteSpace(dto.FinishTime))
                result.FinishTime = dto.FinishTime.Trim();

            if (dto.Position.HasValue)
                result.Position = dto.Position;

            if (dto.Notes != null)
                result.Notes = dto.Notes.Trim();

            await _db.SaveChangesAsync();

            return Ok(MapToDto(result));
        }

        /// <summary>Deletes a result record. Organiser only.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can delete results." });

            var result = await _db.Results.FindAsync(id);

            if (result == null)
                return NotFound(new { message = $"Result with ID {id} not found." });

            _db.Results.Remove(result);
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Result with ID {id} deleted successfully." });
        }

        // ── helpers ───────────────────────────────────────────────────────────
        private static ResultResponseDto MapToDto(Result r) => new()
        {
            ResultID        = r.ResultID,
            EnrolmentID     = r.EnrolmentID,
            ParticipantName = r.Enrolment?.Participant?.FullName ?? string.Empty,
            EventName       = r.Enrolment?.Event?.EventName ?? string.Empty,
            EventDate       = r.Enrolment?.Event?.EventDate ?? DateTime.MinValue,
            CategoryName    = r.Enrolment?.Event?.Category?.CategoryName ?? string.Empty,
            FinishTime      = r.FinishTime,
            Position        = r.Position,
            Notes           = r.Notes
        };
    }
}
