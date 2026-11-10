using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Middleware;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Returns all event categories. Public access.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _db.Categories
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryResponseDto
                {
                    CategoryID   = c.CategoryID,
                    CategoryName = c.CategoryName,
                    Description  = c.Description
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>Returns a single category by ID. Public access.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CategoryResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = $"Category with ID {id} not found." });

            return Ok(new CategoryResponseDto
            {
                CategoryID   = category.CategoryID,
                CategoryName = category.CategoryName,
                Description  = category.Description
            });
        }

        /// <summary>Creates a new event category. Organiser only.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can create categories." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _db.Categories
                .AnyAsync(c => c.CategoryName.ToLower() == dto.CategoryName.ToLower());

            if (exists)
                return Conflict(new { message = "A category with this name already exists." });

            var category = new Category
            {
                CategoryName = dto.CategoryName.Trim(),
                Description  = dto.Description?.Trim()
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var response = new CategoryResponseDto
            {
                CategoryID   = category.CategoryID,
                CategoryName = category.CategoryName,
                Description  = category.Description
            };

            return CreatedAtAction(nameof(GetById), new { id = category.CategoryID }, response);
        }

        /// <summary>Updates an existing category. Organiser only.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CategoryResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
        {
            if (!HttpContext.Session.IsAuthenticated())
                return Unauthorized(new { message = "You must be logged in." });

            if (!HttpContext.Session.IsOrganiser())
                return StatusCode(403, new { message = "Only Organisers can update categories." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = $"Category with ID {id} not found." });

            category.CategoryName = dto.CategoryName.Trim();
            category.Description  = dto.Description?.Trim();

            await _db.SaveChangesAsync();

            return Ok(new CategoryResponseDto
            {
                CategoryID   = category.CategoryID,
                CategoryName = category.CategoryName,
                Description  = category.Description
            });
        }

        /// <summary>Deletes a category by ID. Organiser only.</summary>
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
                return StatusCode(403, new { message = "Only Organisers can delete categories." });

            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = $"Category with ID {id} not found." });

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Category '{category.CategoryName}' deleted successfully." });
        }
    }
}
