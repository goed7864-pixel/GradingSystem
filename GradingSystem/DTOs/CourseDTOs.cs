using System;

namespace GradingSystem.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? AssignmentCount { get; set; }
        public int? StudentCount { get; set; }
    }

    public class CourseCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TeacherId { get; set; }
    }

    public class CourseUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CourseResponse
    {
        public string Message { get; set; } = string.Empty;
        public CourseDto? Course { get; set; }
    }
}
