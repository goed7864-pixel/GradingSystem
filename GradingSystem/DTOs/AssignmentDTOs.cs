using System;

namespace GradingSystem.DTOs
{
    public class AssignmentDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? SubmissionsCount { get; set; }
        public int? GradedCount { get; set; }
    }

    public class AssignmentCreateDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }
        public DateTime Deadline { get; set; }
    }

    public class AssignmentUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }
        public DateTime Deadline { get; set; }
    }

    public class AssignmentResponse
    {
        public string Message { get; set; } = string.Empty;
        public AssignmentDto? Assignment { get; set; }
    }
}
