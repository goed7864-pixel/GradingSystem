using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Models
{
    internal class Grade
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public int Score { get; set; }
        public string Feedback { get; set; }
        public int GradedById { get; set; }
        public DateTime GradedAt { get; set; }

        public Grade()
        {
            GradedAt = DateTime.UtcNow;
        }

        public Grade(int score, int gradedById, string feedback = "")
        {
            if (score < 0 || score > 1000)
                throw new ArgumentOutOfRangeException(nameof(score),
                    "Оценка должна быть от 0 до 1000");

            Score = score;
            GradedById = gradedById;
            Feedback = feedback;
            GradedAt = DateTime.UtcNow;
        }
    }
}
