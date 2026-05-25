using System;
using System.Collections.Generic;

namespace GradingSystem.DTOs
{
    public class TeacherDashboardDto
    {
        public int TotalCourses { get; set; }
        public int TotalAssignments { get; set; }
        public int PendingSubmissions { get; set; }
        public int TotalStudents { get; set; }
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
    }

    public class StudentDashboardDto
    {
        public int EnrolledCourses { get; set; }
        public int TotalAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public double AverageGrade { get; set; }
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
    }

    public class RecentActivityItem
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
