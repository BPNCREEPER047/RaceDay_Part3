using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Middleware;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Produces("application/json")]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EventsController(AppDbContext db) => _db = db;

        /// <summary>Returns all events ordered by date. Public access.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EventResponseDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var events = await _db.Events
                .Include(e => e.Category)
                .Include(e => e.Organiser)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events.Select(MapToDto));
        }

        /// <summary>Returns a single event by ID. Public access.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EventResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _db.Events
                .Include(e => e.Category)
                .Include(e => e.Organiser)
                .Include(e => e.Route)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = $"Event {id} not found." });

            return Ok(MapToDto(ev));
        }

        /// <summary>Creates a new event. Organiser only.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(EventResponseDto), 201)]
        [ProducesResponseType(400)][ProducesResponseType(401)][ProducesResponseType(403)]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });
            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can create events." });
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoryExists = await _db.Categories.AnyAsync(c => c.CategoryID == dto.CategoryID);
            if (!categoryExists)
                return BadRequest(new { message = $"Category {dto.CategoryID} does not exist." });

            var validTypes = new[] { "run", "walk", "cycle" };
            var eventType  = dto.EventType?.ToLower() ?? "run";
            if (!validTypes.Contains(eventType))
                return BadRequest(new { message = "EventType must be 'run', 'walk', or 'cycle'." });

            var ev = new Event
            {
                EventName       = dto.EventName.Trim(),
                Description     = dto.Description?.Trim(),
                EventDate       = dto.EventDate,
                Location        = dto.Location.Trim(),
                Distance        = dto.Distance,
                MaxParticipants = dto.MaxParticipants,
                EventType       = eventType,
                BannerImageUrl  = dto.BannerImageUrl,
                CategoryID      = dto.CategoryID,
                OrganiserID     = HttpContext.Session.GetUserId()!.Value,
                CreatedAt       = DateTime.UtcNow
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            await _db.Entry(ev).Reference(e => e.Category).LoadAsync();
            await _db.Entry(ev).Reference(e => e.Organiser).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.EventID }, MapToDto(ev));
        }

        /// <summary>Updates an event. Organiser only.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(EventResponseDto), 200)]
        [ProducesResponseType(400)][ProducesResponseType(401)][ProducesResponseType(403)][ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateEventDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });
            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can update events." });
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ev = await _db.Events
                .Include(e => e.Category)
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (ev == null)
                return NotFound(new { message = $"Event {id} not found." });

            var categoryExists = await _db.Categories.AnyAsync(c => c.CategoryID == dto.CategoryID);
            if (!categoryExists)
                return BadRequest(new { message = $"Category {dto.CategoryID} does not exist." });

            var validTypes = new[] { "run", "walk", "cycle" };
            var eventType  = dto.EventType?.ToLower() ?? ev.EventType;
            if (!validTypes.Contains(eventType))
                return BadRequest(new { message = "EventType must be 'run', 'walk', or 'cycle'." });

            ev.EventName       = dto.EventName.Trim();
            ev.Description     = dto.Description?.Trim();
            ev.EventDate       = dto.EventDate;
            ev.Location        = dto.Location.Trim();
            ev.Distance        = dto.Distance;
            ev.MaxParticipants = dto.MaxParticipants;
            ev.EventType       = eventType;
            ev.CategoryID      = dto.CategoryID;
            if (dto.BannerImageUrl != null)
                ev.BannerImageUrl = dto.BannerImageUrl;

            await _db.SaveChangesAsync();
            await _db.Entry(ev).Reference(e => e.Category).LoadAsync();

            return Ok(MapToDto(ev));
        }

        /// <summary>Deletes an event. Organiser only.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(200)][ProducesResponseType(401)][ProducesResponseType(403)][ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });
            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can delete events." });

            var ev = await _db.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event {id} not found." });

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return Ok(new { message = $"Event '{ev.EventName}' deleted." });
        }

        /// <summary>Returns all enrolments for an event. Organiser only.</summary>
        [HttpGet("{id:int}/enrolments")]
        [ProducesResponseType(typeof(IEnumerable<EnrolmentResponseDto>), 200)]
        [ProducesResponseType(401)][ProducesResponseType(403)][ProducesResponseType(404)]
        public async Task<IActionResult> GetEventEnrolments(int id)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });
            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can view event enrolments." });

            var eventExists = await _db.Events.AnyAsync(e => e.EventID == id);
            if (!eventExists)
                return NotFound(new { message = $"Event {id} not found." });

            var enrolments = await _db.Enrolments
                .Include(en => en.Participant)
                .Include(en => en.Event)
                    .ThenInclude(ev => ev!.Category)
                .Where(en => en.EventID == id)
                .OrderBy(en => en.EnrolledAt)
                .ToListAsync();

            return Ok(enrolments.Select(MapEnrolmentToDto));
        }

        /// <summary>Upload/update banner image URL for an event. Organiser only.</summary>
        [HttpPost("{id:int}/banner")]
        [ProducesResponseType(200)][ProducesResponseType(401)][ProducesResponseType(403)][ProducesResponseType(404)]
        public async Task<IActionResult> UpdateBanner(int id, [FromBody] UpdateBannerDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });
            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can update banners." });

            var ev = await _db.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = $"Event {id} not found." });

            ev.BannerImageUrl = dto.ImageUrl;
            await _db.SaveChangesAsync();
            return Ok(new { imageUrl = ev.BannerImageUrl });
        }

        // ── helpers ──────────────────────────────────────────────────────────
        internal static EventResponseDto MapToDto(Event e) => new()
        {
            EventID         = e.EventID,
            EventName       = e.EventName,
            Description     = e.Description,
            EventDate       = e.EventDate,
            Location        = e.Location,
            Distance        = e.Distance,
            MaxParticipants = e.MaxParticipants,
            EventType       = e.EventType,
            BannerImageUrl  = e.BannerImageUrl,
            CategoryID      = e.CategoryID,
            CategoryName    = e.Category?.CategoryName ?? string.Empty,
            OrganiserID     = e.OrganiserID,
            OrganiserName   = e.Organiser?.FullName ?? string.Empty,
            CreatedAt       = e.CreatedAt
        };

        internal static EnrolmentResponseDto MapEnrolmentToDto(Enrolment en) => new()
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

    // Simple DTO for banner update
    public class UpdateBannerDto
    {
        public string? ImageUrl { get; set; }
    }
}
