using System;

namespace GradingSystem.DTOs
{
    public class SubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class SubmissionResponse
    {
        public string Message { get; set; } = string.Empty;
        public SubmissionDto? Submission { get; set; }
    }
}
