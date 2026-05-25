using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Models
{
    internal class Teacher : User
    {
        public override string Role => "teacher";
        public List<Course> Courses { get; set; } = new();
        public List<Grade> GradesForStud { get; set; } = new();

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
            return "Преподаватель";
        }

        public override string GetUserName()
        {
            return FullName;
        }
    }
}
