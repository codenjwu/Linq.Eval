using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linq.Eval.Test
{
    internal class Teacher
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public int WorkHours { get; set; }
        public double Salary { get; set; }
        public bool IsPrinciple { get; set; }
        public Student[] Students { get; set; }
    }
    internal class Student
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public Teacher Teacher { get; set; }
    }
}
