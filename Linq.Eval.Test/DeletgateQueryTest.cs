namespace Linq.Eval.Test
{
    [TestClass]
    public class DeletgateQueryTest
    {
        Teacher Teacher1;
        Teacher Teacher2;
        Teacher Teacher3;
        Student[] Students;

        [TestInitialize]
        public void Init()
        {
            Teacher1 = new Teacher() { FirstName = "tf1", LastName = "tl1", Age = 30, IsPrinciple = false, Salary = 32, WorkHours = 7 };
            Teacher2 = new Teacher() { FirstName = "tf4", LastName = "tl4", Age = 38, IsPrinciple = false, Salary = 35, WorkHours = 8 };
            Teacher3 = new Teacher() { FirstName = "tf5", LastName = "tl5", Age = 40, IsPrinciple = true, Salary = 40, WorkHours = 10 };
            Students = new Student[]{
            new Student(){ Age = 10, FirstName = "sf1",LastName="sl1",Teacher = Teacher1 },
            new Student(){ Age = 11, FirstName = "sf2",LastName="sl2", Teacher = Teacher2  },
            new Student(){ Age = 10, FirstName = "sf3",LastName="sl3", Teacher = Teacher1  },
            new Student(){ Age = 9, FirstName = "sf4",LastName="sl4", Teacher = Teacher2  },
            new Student(){ Age = 10, FirstName = "sf5",LastName="sl5", Teacher = Teacher3  },
            };
        }


    }
}