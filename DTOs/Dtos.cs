namespace LMS.API.DTOs;

// ── Auth DTOs ──────────────────────────────────────────────────
public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserDto
{
    public int    Id       { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Role     { get; set; } = string.Empty;
}

// JWT login response — contains both token and user info
public class LoginResponseDto
{
    public string  Token { get; set; } = string.Empty;
    public UserDto User  { get; set; } = new();
}

// ── Course DTOs ────────────────────────────────────────────────
public class CourseCreateDto
{
    public string  Title        { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public string? Category     { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool    IsPublished  { get; set; } = false;
}

public class CourseUpdateDto
{
    public string  Title        { get; set; } = string.Empty;
    public string? Description  { get; set; }
    public string? Category     { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool    IsPublished  { get; set; }
}

public class CourseDto
{
    public int          Id              { get; set; }
    public string       Title           { get; set; } = string.Empty;
    public string?      Description     { get; set; }
    public string?      Category        { get; set; }
    public string?      ThumbnailUrl    { get; set; }
    public bool         IsPublished     { get; set; }
    public DateTime     CreatedAt       { get; set; }
    public string       CreatedBy       { get; set; } = string.Empty;
    public int          LessonCount     { get; set; }
    public int          EnrollmentCount { get; set; }
    public List<LessonDto> Lessons      { get; set; } = new();
}

// ── Lesson DTOs ────────────────────────────────────────────────
public class LessonCreateDto
{
    public string  Title           { get; set; } = string.Empty;
    public string? Content         { get; set; }
    public string? VideoUrl        { get; set; }
    public int     SortOrder       { get; set; } = 0;
    public int     DurationMinutes { get; set; } = 0;
}

public class LessonUpdateDto
{
    public string  Title           { get; set; } = string.Empty;
    public string? Content         { get; set; }
    public string? VideoUrl        { get; set; }
    public int     SortOrder       { get; set; }
    public int     DurationMinutes { get; set; }
}

public class LessonDto
{
    public int     Id              { get; set; }
    public int     CourseId        { get; set; }
    public string  Title           { get; set; } = string.Empty;
    public string? Content         { get; set; }
    public string? VideoUrl        { get; set; }
    public int     SortOrder       { get; set; }
    public int     DurationMinutes { get; set; }
}

// ── Generic Response ───────────────────────────────────────────
public class ApiResponse<T>
{
    public bool   Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T?     Data    { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}
