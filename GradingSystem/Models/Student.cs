using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Models
{
    internal class Student : User
    {
        public override string Role => "student";
        public int? GroupId { get; set; }

        public List<Enrollment> Enrollments { get; set; } = new();

        public List<Submission> Submissions { get; set; } = new();

        public override string GetEmail()
        {
            return Email;
        }

        public override string GetRole()
        {
            return Role;
        }

        public override string GetRoleName()
        {
            return "Студент";
        }

        public override string GetUserName()
        {
            return FullName;
        }
    }
}
