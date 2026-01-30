namespace Linq.Eval.Test
{
    /// <summary>
    /// Tests for DelegateQuery functionality - converting strings to compiled delegates.
    /// </summary>
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
            Teacher1 = new Teacher() { FirstName = "tf1", LastName = "tl1", Age = 30, IsPrinciple = false, Salary = 32000, WorkHours = 7 };
            Teacher2 = new Teacher() { FirstName = "tf4", LastName = "tl4", Age = 38, IsPrinciple = false, Salary = 35000, WorkHours = 8 };
            Teacher3 = new Teacher() { FirstName = "tf5", LastName = "tl5", Age = 40, IsPrinciple = true, Salary = 40000, WorkHours = 10 };
            Students = new Student[]{
                new Student(){ Age = 10, FirstName = "sf1", LastName="sl1", Teacher = Teacher1 },
                new Student(){ Age = 11, FirstName = "sf2", LastName="sl2", Teacher = Teacher2  },
                new Student(){ Age = 10, FirstName = "sf3", LastName="sl3", Teacher = Teacher1  },
                new Student(){ Age = 9, FirstName = "sf4", LastName="sl4", Teacher = Teacher2  },
                new Student(){ Age = 10, FirstName = "sf5", LastName="sl5", Teacher = Teacher3  },
            };
        }

        [TestMethod]
        public async Task Test_Delegate_Where_Simple()
        {
            var predicate = await "x => x.Age == 10".ToDelegate<Func<Student, bool>>();
            var results = Students.Where(predicate).ToArray();
            Assert.AreEqual(3, results.Length);
        }

        [TestMethod]
        public async Task Test_Delegate_Where_Complex()
        {
            var predicate = await "x => x.FirstName == \"sf1\" && x.Teacher.Age > 25".ToDelegate<Func<Student, bool>>();
            var results = Students.Where(predicate).ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("sf1", results[0].FirstName);
        }

        [TestMethod]
        public async Task Test_Delegate_Select_Property()
        {
            var selector = await "x => x.FirstName".ToDelegate<Func<Student, string>>();
            var results = Students.Select(selector).ToArray();
            Assert.AreEqual(5, results.Length);
            Assert.AreEqual("sf1", results[0]);
        }

        [TestMethod]
        public async Task Test_Delegate_Select_Computed()
        {
            var selector = await "x => x.Age * 2".ToDelegate<Func<Student, int>>();
            var results = Students.Select(selector).ToArray();
            Assert.AreEqual(20, results[0]); // 10 * 2
            Assert.AreEqual(22, results[1]); // 11 * 2
        }

        [TestMethod]
        public async Task Test_Delegate_OrderBy()
        {
            var keySelector = await "x => x.Age".ToDelegate<Func<Student, int>>();
            var results = Students.OrderBy(keySelector).ToArray();
            Assert.AreEqual(9, results[0].Age);
            Assert.AreEqual(11, results[4].Age);
        }

        [TestMethod]
        public async Task Test_Delegate_OrderByDescending()
        {
            var keySelector = await "x => x.Age".ToDelegate<Func<Student, int>>();
            var results = Students.OrderByDescending(keySelector).ToArray();
            Assert.AreEqual(11, results[0].Age);
            Assert.AreEqual(9, results[4].Age);
        }

        [TestMethod]
        public async Task Test_Delegate_ThenBy()
        {
            var firstKey = await "x => x.Age".ToDelegate<Func<Student, int>>();
            var secondKey = await "x => x.FirstName".ToDelegate<Func<Student, string>>();
            var results = Students.OrderBy(firstKey).ThenBy(secondKey).ToArray();
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Length);
        }

        [TestMethod]
        public async Task Test_Delegate_Any_WithPredicate()
        {
            var predicate = await "x => x.Age > 10".ToDelegate<Func<Student, bool>>();
            var result = Students.Any(predicate);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Test_Delegate_Any_NoMatch()
        {
            var predicate = await "x => x.Age > 100".ToDelegate<Func<Student, bool>>();
            var result = Students.Any(predicate);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task Test_Delegate_All()
        {
            var predicate = await "x => x.Age > 0".ToDelegate<Func<Student, bool>>();
            var result = Students.All(predicate);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Test_Delegate_All_False()
        {
            var predicate = await "x => x.Age > 10".ToDelegate<Func<Student, bool>>();
            var result = Students.All(predicate);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task Test_Delegate_Count()
        {
            var predicate = await "x => x.Age == 10".ToDelegate<Func<Student, bool>>();
            var result = Students.Count(predicate);
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public async Task Test_Delegate_First()
        {
            var predicate = await "x => x.FirstName == \"sf2\"".ToDelegate<Func<Student, bool>>();
            var result = Students.First(predicate);
            Assert.AreEqual(11, result.Age);
        }

        [TestMethod]
        public async Task Test_Delegate_FirstOrDefault()
        {
            var predicate = await "x => x.FirstName == \"notfound\"".ToDelegate<Func<Student, bool>>();
            var result = Students.FirstOrDefault(predicate);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task Test_Delegate_Single()
        {
            var predicate = await "x => x.FirstName == \"sf2\"".ToDelegate<Func<Student, bool>>();
            var result = Students.Single(predicate);
            Assert.AreEqual(11, result.Age);
        }

        [TestMethod]
        public async Task Test_Delegate_GroupBy()
        {
            var keySelector = await "x => x.Age".ToDelegate<Func<Student, int>>();
            var groups = Students.GroupBy(keySelector).ToArray();
            Assert.AreEqual(3, groups.Length); // Ages: 9, 10, 11
        }

        [TestMethod]
        public async Task Test_Delegate_Distinct()
        {
            var selector = await "x => x.Age".ToDelegate<Func<Student, int>>();
            var results = Students.Select(selector).Distinct().ToArray();
            Assert.AreEqual(3, results.Length);
        }

        [TestMethod]
        public async Task Test_Delegate_Skip()
        {
            var results = Students.Skip(2).ToArray();
            Assert.AreEqual(3, results.Length);
        }

        [TestMethod]
        public async Task Test_Delegate_Take()
        {
            var results = Students.Take(2).ToArray();
            Assert.AreEqual(2, results.Length);
        }

        [TestMethod]
        public async Task Test_Delegate_SkipWhile()
        {
            var predicate = await "x => x.Age == 10".ToDelegate<Func<Student, bool>>();
            var results = Students.SkipWhile(predicate).ToArray();
            Assert.IsTrue(results.Length > 0);
        }

        [TestMethod]
        public async Task Test_Delegate_TakeWhile()
        {
            var predicate = await "x => x.Age == 10".ToDelegate<Func<Student, bool>>();
            var results = Students.TakeWhile(predicate).ToArray();
            Assert.IsTrue(results.Length >= 0);
        }

        [TestMethod]
        public async Task Test_Delegate_WithCache_Enabled()
        {
            var query = "x => x.Age > 10";
            var delegate1 = await query.ToDelegate<Func<Student, bool>>(cache: true);
            var delegate2 = await query.ToDelegate<Func<Student, bool>>(cache: true);
            
            var results1 = Students.Where(delegate1).ToArray();
            var results2 = Students.Where(delegate2).ToArray();
            
            Assert.AreEqual(results1.Length, results2.Length);
        }

        [TestMethod]
        public async Task Test_Delegate_NestedProperty()
        {
            var predicate = await "x => x.Teacher.IsPrinciple == true".ToDelegate<Func<Student, bool>>();
            var results = Students.Where(predicate).ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("sf5", results[0].FirstName);
        }

        [TestMethod]
        public async Task Test_Delegate_ComplexBoolean()
        {
            var predicate = await "x => (x.Age > 9 && x.Age < 11) || x.Teacher.Age > 35".ToDelegate<Func<Student, bool>>();
            var results = Students.Where(predicate).ToArray();
            Assert.IsTrue(results.Length > 0);
        }

        [TestMethod]
        public async Task Test_Delegate_StringOperations()
        {
            var selector = await "x => x.FirstName + \" \" + x.LastName".ToDelegate<Func<Student, string>>();
            var results = Students.Select(selector).ToArray();
            Assert.AreEqual("sf1 sl1", results[0]);
        }

        [TestMethod]
        public async Task Test_Delegate_Arithmetic()
        {
            var selector = await "x => (x.Age + 5) * 2".ToDelegate<Func<Student, int>>();
            var results = Students.Select(selector).ToArray();
            Assert.AreEqual(30, results[0]); // (10 + 5) * 2
        }
    }
}
