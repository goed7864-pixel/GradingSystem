using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Models
{
    internal class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Assignment> Assignments { get; set; } = new();
    }


}

