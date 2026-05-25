using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Models
{
    internal class Submission
    {

            public int Id { get; set; }

            public int AssignmentId { get; set; }

            public int StudentId { get; set; }

            public string FilePath { get; set; }

            public string OriginalFileName { get; set; }

            public long FileSize { get; set; }

            public DateTime SubmittedAt { get; set; }

            public Assignment Assignment { get; set; }

            public Student Student { get; set; }

            public Grade Grade { get; set; }

    }
}
