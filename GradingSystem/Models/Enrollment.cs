using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Models
{
    internal class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public Student Student { get; set; }
        public Course Course { get; set; }
        public DateTime EnrolledAt { get; set; }

        public Enrollment()
        {
            EnrolledAt = DateTime.UtcNow;
        }

        public Enrollment(Student student, Course course)
        {
            Student = student;
            Course = course;
            StudentId = student.Id;
            CourseId = course.Id;
            EnrolledAt = DateTime.UtcNow;
        }
    }
}
