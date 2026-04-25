using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;

public interface ICourseService
{
    Task<List<CourseDto>> GetAllAsync(bool publishedOnly = false);
    Task<CourseDto?> GetByIdAsync(int id);
    Task<CourseDto> CreateAsync(CourseCreateDto dto, int adminUserId);
    Task<CourseDto?> UpdateAsync(int id, CourseUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

public class CourseService : ICourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CourseDto>> GetAllAsync(bool publishedOnly = false)
    {
        var query = _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (publishedOnly)
            query = query.Where(c => c.IsPublished);

        var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return courses.Select(MapToDto).ToList();
    }

    public async Task<CourseDto?> GetByIdAsync(int id)
    {
        var course = await _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Lessons.OrderBy(l => l.SortOrder))
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id);

        return course == null ? null : MapToDto(course);
    }

    public async Task<CourseDto> CreateAsync(CourseCreateDto dto, int adminUserId)
    {
        var course = new Course
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Category = dto.Category?.Trim(),
            ThumbnailUrl = dto.ThumbnailUrl?.Trim(),
            IsPublished = dto.IsPublished,
            CreatedByUserId = adminUserId
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        return (await GetByIdAsync(course.Id))!;
    }

    public async Task<CourseDto?> UpdateAsync(int id, CourseUpdateDto dto)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return null;

        course.Title = dto.Title.Trim();
        course.Description = dto.Description?.Trim();
        course.Category = dto.Category?.Trim();
        course.ThumbnailUrl = dto.ThumbnailUrl?.Trim();
        course.IsPublished = dto.IsPublished;
        course.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _db.Courses.FindAsync(id);
        if (course == null) return false;

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
        return true;
    }

    private static CourseDto MapToDto(Course c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Description = c.Description,
        Category = c.Category,
        ThumbnailUrl = c.ThumbnailUrl,
        IsPublished = c.IsPublished,
        CreatedAt = c.CreatedAt,
        CreatedBy = c.CreatedBy?.FullName ?? "Unknown",
        LessonCount = c.Lessons.Count,
        EnrollmentCount = c.Enrollments.Count,
        Lessons = c.Lessons.Select(l => new LessonDto
        {
            Id = l.Id,
            CourseId = l.CourseId,
            Title = l.Title,
            Content = l.Content,
            VideoUrl = l.VideoUrl,
            SortOrder = l.SortOrder,
            DurationMinutes = l.DurationMinutes
        }).ToList()
    };
}
