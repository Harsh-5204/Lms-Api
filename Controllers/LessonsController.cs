using LMS.API.DTOs;
using LMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/courses/{courseId}/lessons")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    // GET /api/courses/{courseId}/lessons
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(int courseId)
    {
        var lessons = await _lessonService.GetByCourseIdAsync(courseId);
        return Ok(ApiResponse<List<LessonDto>>.Ok(lessons));
    }

    // GET /api/courses/{courseId}/lessons/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int courseId, int id)
    {
        var lesson = await _lessonService.GetByIdAsync(id);
        if (lesson == null || lesson.CourseId != courseId)
            return NotFound(ApiResponse<string>.Fail("Lesson not found."));

        return Ok(ApiResponse<LessonDto>.Ok(lesson));
    }

    // POST /api/courses/{courseId}/lessons → Admin only
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(int courseId, [FromBody] LessonCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<string>.Fail("Invalid input."));

        var lesson = await _lessonService.CreateAsync(courseId, dto);
        if (lesson == null)
            return NotFound(ApiResponse<string>.Fail("Course not found."));

        return Created("", ApiResponse<LessonDto>.Ok(lesson, "Lesson created."));
    }

    // PUT /api/courses/{courseId}/lessons/{id} → Admin only
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int courseId, int id, [FromBody] LessonUpdateDto dto)
    {
        var lesson = await _lessonService.UpdateAsync(id, dto);
        if (lesson == null)
            return NotFound(ApiResponse<string>.Fail("Lesson not found."));

        return Ok(ApiResponse<LessonDto>.Ok(lesson, "Lesson updated."));
    }

    // DELETE /api/courses/{courseId}/lessons/{id} → Admin only
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int courseId, int id)
    {
        var deleted = await _lessonService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<string>.Fail("Lesson not found."));

        return Ok(ApiResponse<string>.Ok("", "Lesson deleted."));
    }
}
