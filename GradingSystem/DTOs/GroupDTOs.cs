using System;

namespace GradingSystem.DTOs
{
    public class StudentGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? StudentCount { get; set; }
    }

    public class StudentGroupCreateDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class StudentGroupUpdateDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class StudentGroupResponse
    {
        public string Message { get; set; } = string.Empty;
        public StudentGroupDto? Group { get; set; }
    }
}
