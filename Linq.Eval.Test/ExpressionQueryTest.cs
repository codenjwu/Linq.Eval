namespace Linq.Eval.Test
{
    [TestClass]
    public class ExpressionQueryTest
    {
        Teacher Teacher1;
        Teacher Teacher2;
        Teacher Teacher3;
        Student[] Students1;
        Student[] Students2;
        Student[] Students3;
        Teacher[] Teachers;

        [TestInitialize]
        public void Init()
        {
            Teacher1 = new Teacher() { FirstName = "tf1", LastName = "tl1", Age = 30,   Salary = 32, WorkHours = 7 };
            Teacher2 = new Teacher() { FirstName = "tf4", LastName = "tl4", IsPrinciple = false, Salary = 35, WorkHours = 8 };
            Teacher3 = new Teacher() { FirstName = "tf5", LastName = "tl5", Age = 40, IsPrinciple = true, Salary = 40, WorkHours = 10 };
            Students1 = new Student[]{
            new Student(){ Age = 10, FirstName = "sf1",LastName="sl1",Teacher = Teacher1 },
            new Student(){ Age = 11, FirstName = "sf2",LastName="sl2", Teacher = Teacher2  },
            new Student(){ Age = 10, FirstName = "sf3",LastName="sl3", Teacher = Teacher1  },
            new Student(){ Age = 9, FirstName = "sf4",LastName="sl4", Teacher = Teacher2  },
            new Student(){ Age = 10, FirstName = "sf5",LastName="sl5", Teacher = Teacher3  },
            };
            Students2 = new Student[]{
            new Student(){ Age = 20, FirstName = "sf21",LastName="sl21",Teacher = Teacher1 },
            new Student(){ Age = 21, FirstName = "sf22",LastName="sl22",    },
            new Student(){ Age = 20, FirstName = "sf23",LastName="sl23", Teacher = Teacher1  },
            new Student(){ Age = 29, FirstName = "sf24",LastName="sl4", Teacher = Teacher1  },
            new Student(){ Age = 20, FirstName = "sf25",LastName="sl5", Teacher = Teacher3  },
            };
            Students3 = new Student[]{
            new Student(){ Age = 30, FirstName = "sf31",LastName="sl31",Teacher = Teacher1 },
            new Student(){ Age = 31, FirstName = "sf32",LastName="sl32", Teacher = Teacher2  },
            new Student(){ Age = 30, FirstName = "sf33",LastName="sl33",   },
            new Student(){ Age = 39, FirstName = "sf34",LastName="sl4", Teacher = Teacher3  },
            new Student(){ Age = 30, FirstName = "sf35",LastName="sl5", Teacher = Teacher3  },
            };
            Teacher1.Students = Students1;
            Teacher2.Students = Students2;
            Teacher3.Students = Students3;

            Teachers = new Teacher[] { Teacher1, Teacher2, Teacher3 };
        }

        [TestMethod]
        public void TestMethod()
        {
            var tc1 = Students1.Where("x=>x.FirstName == \"sf1\" && (x.Teacher?.Age??100) > 35".ToExpression<Func<Student, bool>>().Compile()).ToArray();

            var tc2 = Students2.Where("x=>(x.FirstName == (\"s\"+\"f1\") || (x.Teacher?.Age??100 )> 35) && !(x.Teacher?.IsPrinciple??true)  || (x.Age == (x.Teacher?.Age??20) )".ToExpression<Func<Student, bool>>().Compile()).ToArray();

            var tc3 = Students3.Select("x=>x.Teacher?.Age".ToExpression<Func<Student,int?>>().Compile()).ToArray();
        }
    }
}