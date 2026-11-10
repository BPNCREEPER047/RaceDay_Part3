using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Middleware;
using RaceDayAPI.Models;
using Route = RaceDayAPI.Models.Route;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/routes")]
    [Produces("application/json")]
    public class RoutesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public RoutesController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Returns route information for a specific event. Public access.</summary>
        [HttpGet("{eventId:int}")]
        [ProducesResponseType(typeof(RouteResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByEventId(int eventId)
        {
            var route = await _db.Routes
                .FirstOrDefaultAsync(r => r.EventID == eventId);

            if (route == null)
                return NotFound(new { message = $"No route found for event ID {eventId}." });

            return Ok(MapToDto(route));
        }

        /// <summary>Adds route information for an event. Organiser only.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(RouteResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Create([FromBody] CreateRouteDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can add route information." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var eventExists = await _db.Events.AnyAsync(e => e.EventID == dto.EventID);
            if (!eventExists)
                return NotFound(new { message = $"Event with ID {dto.EventID} not found." });

            var routeExists = await _db.Routes
                .AnyAsync(r => r.EventID == dto.EventID);

            if (routeExists)
                return Conflict(new { message = "A route already exists for this event." });

            var route = new Route
            {
                EventID       = dto.EventID,
                RouteMapURL   = dto.RouteMapURL?.Trim(),
                StartPoint    = dto.StartPoint.Trim(),
                EndPoint      = dto.EndPoint.Trim(),
                ElevationGain = dto.ElevationGain,
                Description   = dto.Description?.Trim()
            };

            _db.Routes.Add(route);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEventId),
                new { eventId = route.EventID }, MapToDto(route));
        }

        /// <summary>Updates route information for an event. Organiser only.</summary>
        [HttpPut("{eventId:int}")]
        [ProducesResponseType(typeof(RouteResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int eventId, [FromBody] UpdateRouteDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can update route information." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var route = await _db.Routes
                .FirstOrDefaultAsync(r => r.EventID == eventId);

            if (route == null)
                return NotFound(new { message = $"No route found for event ID {eventId}." });

            if (dto.RouteMapURL != null) route.RouteMapURL   = dto.RouteMapURL.Trim();
            if (!string.IsNullOrWhiteSpace(dto.StartPoint))  route.StartPoint    = dto.StartPoint.Trim();
            if (!string.IsNullOrWhiteSpace(dto.EndPoint))    route.EndPoint      = dto.EndPoint.Trim();
            if (dto.ElevationGain.HasValue)                  route.ElevationGain = dto.ElevationGain;
            if (dto.Description != null)                     route.Description   = dto.Description.Trim();

            await _db.SaveChangesAsync();

            return Ok(MapToDto(route));
        }

        // ── helpers ───────────────────────────────────────────────────────────
        private static RouteResponseDto MapToDto(Route r) => new()
        {
            RouteID       = r.RouteID,
            EventID       = r.EventID,
            RouteMapURL   = r.RouteMapURL,
            StartPoint    = r.StartPoint,
            EndPoint      = r.EndPoint,
            ElevationGain = r.ElevationGain,
            Description   = r.Description
        };
    }
}
