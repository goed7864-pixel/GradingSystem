using System;

namespace GradingSystem.DTOs
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }

    public class EnrollmentCreateDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
    }

    public class EnrollmentResponse
    {
        public string Message { get; set; } = string.Empty;
        public EnrollmentDto? Enrollment { get; set; }
    }
}
