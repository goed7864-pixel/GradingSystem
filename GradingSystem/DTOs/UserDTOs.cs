using System;

namespace GradingSystem.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? Password { get; set; }
        public int? GroupId { get; set; }
    }

    public class UserResponse
    {
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }
    }
}
