namespace GradingSystem.Pages
{
    public class GroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StudentCount { get; set; }
    }

    public class StudentViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public string AvatarColor { get; set; }
        public string Initials { get; set; }
    }

    public class ActivityViewModel
    {
        public string Icon { get; set; }
        public string IconBackground { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Time { get; set; }
    }

    public class DeadlineViewModel
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string CourseName { get; set; }
        public int DaysLeft { get; set; }
    }

    public class HomePageCourseViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Progress { get; set; }
    }

    public class StudentAssignmentCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CourseName { get; set; }
        public string Deadline { get; set; }
        public int MaxScore { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public DateTime DeadlineDate { get; set; }
    }
}
