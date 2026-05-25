using System;

namespace GradingSystem.DTOs
{
    public class GradeDto
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public int Score { get; set; }
        public string? Feedback { get; set; }
        public int GradedById { get; set; }
        public DateTime GradedAt { get; set; }
    }

    public class GradeCreateDto
    {
        public int SubmissionId { get; set; }
        public int Score { get; set; }
        public string? Feedback { get; set; }
        public int GradedById { get; set; }
    }

    public class GradeUpdateDto
    {
        public int Score { get; set; }
        public string? Feedback { get; set; }
    }

    public class GradeResponse
    {
        public string Message { get; set; } = string.Empty;
        public GradeDto? Grade { get; set; }
    }
}
