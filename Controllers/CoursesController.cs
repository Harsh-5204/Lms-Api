using LMS.API.DTOs;
using LMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // GET /api/courses  → All users (published only for non-admin)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var isAdmin = User.IsInRole("Admin");
        var courses = await _courseService.GetAllAsync(publishedOnly: !isAdmin);
        return Ok(ApiResponse<List<CourseDto>>.Ok(courses));
    }

    // GET /api/courses/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course == null)
            return NotFound(ApiResponse<string>.Fail("Course not found."));

        // Non-admin can only view published courses
        if (!User.IsInRole("Admin") && !course.IsPublished)
            return Forbid();

        return Ok(ApiResponse<CourseDto>.Ok(course));
    }

    // POST /api/courses → Admin only
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid input."));

        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var course = await _courseService.CreateAsync(dto, adminId);
        return Created($"/api/courses/{course.Id}", ApiResponse<CourseDto>.Ok(course, "Course created."));
    }

    // PUT /api/courses/{id} → Admin only
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid input."));

        var course = await _courseService.UpdateAsync(id, dto);
        if (course == null)
            return NotFound(ApiResponse<string>.Fail("Course not found."));

        return Ok(ApiResponse<CourseDto>.Ok(course, "Course updated."));
    }

    // DELETE /api/courses/{id} → Admin only
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _courseService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<string>.Fail("Course not found."));

        return Ok(ApiResponse<string>.Ok("", "Course deleted."));
    }
}
