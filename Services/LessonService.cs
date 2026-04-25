using LMS.API.Data;
using LMS.API.DTOs;
using LMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;

public interface ILessonService
{
    Task<List<LessonDto>> GetByCourseIdAsync(int courseId);
    Task<LessonDto?> GetByIdAsync(int id);
    Task<LessonDto?> CreateAsync(int courseId, LessonCreateDto dto);
    Task<LessonDto?> UpdateAsync(int id, LessonUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

public class LessonService : ILessonService
{
    private readonly AppDbContext _db;

    public LessonService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LessonDto>> GetByCourseIdAsync(int courseId)
    {
        return await _db.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.SortOrder)
            .Select(l => MapToDto(l))
            .ToListAsync();
    }

    public async Task<LessonDto?> GetByIdAsync(int id)
    {
        var lesson = await _db.Lessons.FindAsync(id);
        return lesson == null ? null : MapToDto(lesson);
    }

    public async Task<LessonDto?> CreateAsync(int courseId, LessonCreateDto dto)
    {
        var courseExists = await _db.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists) return null;

        var lesson = new Lesson
        {
            CourseId = courseId,
            Title = dto.Title.Trim(),
            Content = dto.Content?.Trim(),
            VideoUrl = dto.VideoUrl?.Trim(),
            SortOrder = dto.SortOrder,
            DurationMinutes = dto.DurationMinutes
        };

        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();
        return MapToDto(lesson);
    }

    public async Task<LessonDto?> UpdateAsync(int id, LessonUpdateDto dto)
    {
        var lesson = await _db.Lessons.FindAsync(id);
        if (lesson == null) return null;

        lesson.Title = dto.Title.Trim();
        lesson.Content = dto.Content?.Trim();
        lesson.VideoUrl = dto.VideoUrl?.Trim();
        lesson.SortOrder = dto.SortOrder;
        lesson.DurationMinutes = dto.DurationMinutes;

        await _db.SaveChangesAsync();
        return MapToDto(lesson);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var lesson = await _db.Lessons.FindAsync(id);
        if (lesson == null) return false;

        _db.Lessons.Remove(lesson);
        await _db.SaveChangesAsync();
        return true;
    }

    private static LessonDto MapToDto(Lesson l) => new()
    {
        Id = l.Id,
        CourseId = l.CourseId,
        Title = l.Title,
        Content = l.Content,
        VideoUrl = l.VideoUrl,
        SortOrder = l.SortOrder,
        DurationMinutes = l.DurationMinutes
    };
}
