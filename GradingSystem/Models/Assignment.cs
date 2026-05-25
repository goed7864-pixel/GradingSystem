using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Models
{
    internal class Assignment
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public Course Course { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int MaxScore { get; set; }

        public DateTime Deadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Submission> Submissions { get; set; } = new();
    }
}
