namespace Linq.Eval.Test
{
    /// <summary>
    /// Tests for two-parameter lambda expressions and complex scenarios.
    /// </summary>
    [TestClass]
    public class TwoParameterLambdaTests
    {
        private Teacher[] teachers = null!;
        private Student[] students = null!;

        [TestInitialize]
        public void Init()
        {
            var teacher1 = new Teacher
            {
                FirstName = "John",
                LastName = "Smith",
                Age = 30,
                IsPrinciple = false,
                Salary = 50000,
                WorkHours = 40
            };

            var teacher2 = new Teacher
            {
                FirstName = "Alice",
                LastName = "Johnson",
                Age = 45,
                IsPrinciple = true,
                Salary = 75000,
                WorkHours = 45
            };

            var teacher3 = new Teacher
            {
                FirstName = "Bob",
                LastName = "Williams",
                Age = 35,
                IsPrinciple = false,
                Salary = 55000,
                WorkHours = 38
            };

            students = new Student[]
            {
                new Student { Age = 18, FirstName = "Tom", LastName = "Brown", Teacher = teacher1 },
                new Student { Age = 20, FirstName = "Jane", LastName = "Davis", Teacher = teacher2 },
                new Student { Age = 19, FirstName = "Mike", LastName = "Wilson", Teacher = teacher3 },
            };

            teachers = new[] { teacher1, teacher2, teacher3 };
        }

        #region Basic Two-Parameter Operations

        [TestMethod]
        public void Test_TwoParameters_Addition()
        {
            var expr = "(x, y) => x + y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(5, compiled(2, 3));
            Assert.AreEqual(100, compiled(50, 50));
            Assert.AreEqual(0, compiled(-5, 5));
        }

        [TestMethod]
        public void Test_TwoParameters_Subtraction()
        {
            var expr = "(x, y) => x - y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(5, compiled(10, 5));
            Assert.AreEqual(-5, compiled(5, 10));
            Assert.AreEqual(0, compiled(10, 10));
        }

        [TestMethod]
        public void Test_TwoParameters_Multiplication()
        {
            var expr = "(x, y) => x * y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(20, compiled(4, 5));
            Assert.AreEqual(0, compiled(0, 10));
            Assert.AreEqual(-20, compiled(-4, 5));
        }

        [TestMethod]
        public void Test_TwoParameters_Division()
        {
            var expr = "(x, y) => x / y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(5, compiled(10, 2));
            Assert.AreEqual(3, compiled(10, 3)); // Integer division
            Assert.AreEqual(-2, compiled(-10, 5));
        }

        [TestMethod]
        public void Test_TwoParameters_Modulo()
        {
            var expr = "(x, y) => x % y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(1, compiled(10, 3));
            Assert.AreEqual(0, compiled(10, 5));
            Assert.AreEqual(2, compiled(12, 5));
        }

        #endregion

        #region Complex Arithmetic with Two Parameters

        [TestMethod]
        public void Test_TwoParameters_ComplexArithmetic()
        {
            var expr = "(x, y) => (x + y) * (x - y)"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(21, compiled(5, 2)); // (5+2) * (5-2) = 7 * 3 = 21
            Assert.AreEqual(0, compiled(5, 5)); // (5+5) * (5-5) = 10 * 0 = 0
        }

        [TestMethod]
        public void Test_TwoParameters_NestedArithmetic()
        {
            var expr = "(x, y) => ((x * 2) + (y * 3)) / 2"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(8, compiled(5, 2)); // ((5*2) + (2*3)) / 2 = (10 + 6) / 2 = 8
            Assert.AreEqual(10, compiled(4, 4)); // ((4*2) + (4*3)) / 2 = (8 + 12) / 2 = 10
        }

        [TestMethod]
        public void Test_TwoParameters_PowerLikeOperation()
        {
            var expr = "(x, y) => x * x + y * y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(25, compiled(3, 4)); // 9 + 16 = 25
            Assert.AreEqual(8, compiled(2, 2)); // 4 + 4 = 8
        }

        #endregion

        #region Comparison Operations

        [TestMethod]
        public void Test_TwoParameters_GreaterThan()
        {
            var expr = "(x, y) => x > y"
                .ToExpression<Func<int, int, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(10, 5));
            Assert.IsFalse(compiled(5, 10));
            Assert.IsFalse(compiled(5, 5));
        }

        [TestMethod]
        public void Test_TwoParameters_Equality()
        {
            var expr = "(x, y) => x == y"
                .ToExpression<Func<int, int, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(5, 5));
            Assert.IsFalse(compiled(5, 10));
        }

        [TestMethod]
        public void Test_TwoParameters_ComplexComparison()
        {
            var expr = "(x, y) => x > 10 && y < 20"
                .ToExpression<Func<int, int, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(15, 10));
            Assert.IsFalse(compiled(5, 10));
            Assert.IsFalse(compiled(15, 25));
        }

        [TestMethod]
        public void Test_TwoParameters_RangeCheck()
        {
            var expr = "(x, y) => x >= y && x <= y + 10"
                .ToExpression<Func<int, int, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(15, 10)); // 15 >= 10 && 15 <= 20
            Assert.IsFalse(compiled(25, 10)); // 25 >= 10 but 25 > 20
            Assert.IsTrue(compiled(10, 10)); // 10 >= 10 && 10 <= 20
        }

        #endregion

        #region Conditional Operations

        [TestMethod]
        public void Test_TwoParameters_Conditional()
        {
            var expr = "(x, y) => x > y ? x : y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(10, compiled(10, 5)); // Max of 10 and 5
            Assert.AreEqual(10, compiled(5, 10)); // Max of 5 and 10
            Assert.AreEqual(5, compiled(5, 5)); // Equal values
        }

        [TestMethod]
        public void Test_TwoParameters_NestedConditional()
        {
            var expr = "(x, y) => x > y ? (x > 10 ? 100 : 50) : (y > 10 ? 200 : 25)"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(100, compiled(15, 5)); // x > y, x > 10
            Assert.AreEqual(50, compiled(8, 5)); // x > y, x <= 10
            Assert.AreEqual(200, compiled(5, 15)); // x <= y, y > 10
            Assert.AreEqual(25, compiled(5, 8)); // x <= y, y <= 10
        }

        [TestMethod]
        public void Test_TwoParameters_ConditionalWithArithmetic()
        {
            var expr = "(x, y) => x > y ? x + y : x - y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(15, compiled(10, 5)); // 10 + 5
            Assert.AreEqual(-5, compiled(5, 10)); // 5 - 10
        }

        #endregion

        #region Object Parameter Tests

        [TestMethod]
        public void Test_TwoObjects_CompareProperty()
        {
            var expr = "(x, y) => (x.Age ?? 0) > (y.Age ?? 0)"
                .ToExpression<Func<Teacher, Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0], teachers[1])); // 30 > 45 = false
            Assert.IsTrue(compiled(teachers[1], teachers[0])); // 45 > 30 = true
        }

        [TestMethod]
        public void Test_TwoObjects_SumProperties()
        {
            var expr = "(x, y) => (x.Age ?? 0) + (y.Age ?? 0)"
                .ToExpression<Func<Teacher, Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(75, compiled(teachers[0], teachers[1])); // 30 + 45
            Assert.AreEqual(65, compiled(teachers[0], teachers[2])); // 30 + 35
        }

        [TestMethod]
        public void Test_TwoObjects_ComplexPropertyOperation()
        {
            var expr = "(x, y) => (x.Salary ?? 0) + (y.Salary ?? 0)"
                .ToExpression<Func<Teacher, Teacher, double>>();
            var compiled = expr.Compile();

            Assert.AreEqual(125000.0, compiled(teachers[0], teachers[1])); // 50000 + 75000
            Assert.AreEqual(105000.0, compiled(teachers[0], teachers[2])); // 50000 + 55000
        }

        [TestMethod]
        public void Test_TwoObjects_ConditionalComparison()
        {
            var expr = "(x, y) => (x.Age ?? 0) > (y.Age ?? 0) ? (x.Salary ?? 0) : (y.Salary ?? 0)"
                .ToExpression<Func<Teacher, Teacher, double>>();
            var compiled = expr.Compile();

            Assert.AreEqual(75000.0, compiled(teachers[0], teachers[1])); // 30 < 45, return y
            Assert.AreEqual(75000.0, compiled(teachers[1], teachers[0])); // 45 > 30, return x
        }

        [TestMethod]
        public void Test_TwoObjects_MultipleProperties()
        {
            var expr = "(x, y) => ((x.Age ?? 0) + (y.Age ?? 0)) * (x.WorkHours + y.WorkHours)"
                .ToExpression<Func<Teacher, Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(6375, compiled(teachers[0], teachers[1])); // (30+45) * (40+45) = 75 * 85 = 6375
        }

        #endregion

        #region Mixed Type Parameters

        [TestMethod]
        public void Test_ObjectAndInt_Addition()
        {
            var expr = "(x, y) => (x.Age ?? 0) + y"
                .ToExpression<Func<Teacher, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(40, compiled(teachers[0], 10)); // 30 + 10
            Assert.AreEqual(50, compiled(teachers[1], 5)); // 45 + 5
        }

        [TestMethod]
        public void Test_ObjectAndInt_Comparison()
        {
            var expr = "(x, y) => (x.Age ?? 0) > y"
                .ToExpression<Func<Teacher, int, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0], 25)); // 30 > 25
            Assert.IsFalse(compiled(teachers[0], 35)); // 30 > 35
        }

        [TestMethod]
        public void Test_ObjectAndInt_Conditional()
        {
            var expr = "(x, y) => (double)(x.Age ?? 0) > y ? (x.Salary ?? 0.0) : y"
                .ToExpression<Func<Teacher, double, double>>();
            var compiled = expr.Compile();

            Assert.AreEqual(50000.0, compiled(teachers[0], 25.0)); // Age 30 > 25, returns Salary
            Assert.AreEqual(50.0, compiled(teachers[0], 50.0)); // Age 30 < 50, returns y
        }

        #endregion

        #region String Parameters

        [TestMethod]
        public void Test_TwoStrings_Equality()
        {
            var expr = "(x, y) => x == y"
                .ToExpression<Func<string, string, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled("Hello", "Hello"));
            Assert.IsFalse(compiled("Hello", "World"));
        }

        [TestMethod]
        public void Test_ObjectAndString_PropertyComparison()
        {
            var expr = "(x, y) => x.FirstName == y"
                .ToExpression<Func<Teacher, string, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0], "John"));
            Assert.IsFalse(compiled(teachers[0], "Alice"));
        }

        [TestMethod]
        public void Test_TwoObjects_StringPropertyComparison()
        {
            var expr = "(x, y) => x.FirstName == y.FirstName"
                .ToExpression<Func<Teacher, Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0], teachers[1])); // John != Alice
            Assert.IsTrue(compiled(teachers[0], teachers[0])); // John == John
        }

        [TestMethod]
        public void Test_StringAndInt_Conditional()
        {
            var expr = "(x, y) => x.FirstName == \"John\" ? y * 2 : y"
                .ToExpression<Func<Teacher, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(20, compiled(teachers[0], 10)); // John: 10 * 2
            Assert.AreEqual(10, compiled(teachers[1], 10)); // Alice: 10
        }

        #endregion

        #region Complex Scenarios

        [TestMethod]
        public void Test_TwoParameters_ComplexLogic()
        {
            var expr = "(x, y) => (x > y && x > 10) || (y > x && y > 10)"
                .ToExpression<Func<int, int, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(15, 5)); // 15 > 5 && 15 > 10
            Assert.IsTrue(compiled(5, 15)); // 15 > 5 && 15 > 10
            Assert.IsFalse(compiled(8, 5)); // 8 > 5 but 8 <= 10
            Assert.IsFalse(compiled(5, 5)); // Equal
        }

        [TestMethod]
        public void Test_TwoObjects_ComplexExpression()
        {
            var expr = "(x, y) => ((x.Age ?? 0) < (y.Age ?? 0) ? (y.Salary ?? 0.0) : (x.Salary ?? 0.0)) / (((double)x.WorkHours + (double)y.WorkHours) / 2.0)"
                .ToExpression<Func<Teacher, Teacher, double>>();
            var compiled = expr.Compile();

            // teachers[0] (30, 50000, 40) vs teachers[1] (45, 75000, 45)
            // Age: 30 < 45, so Salary = 75000
            // WorkHours: (40 + 45) / 2.0 = 42.5
            // Result: 75000 / 42.5 = 1764.7058823529412
            Assert.AreEqual(1764.7058823529412, compiled(teachers[0], teachers[1]), 0.0001);
        }

        [TestMethod]
        public void Test_ThreeWayComparison()
        {
            var expr = "(x, y) => x > y ? 1 : (x < y ? -1 : 0)"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(1, compiled(10, 5)); // x > y
            Assert.AreEqual(-1, compiled(5, 10)); // x < y
            Assert.AreEqual(0, compiled(5, 5)); // x == y
        }

        [TestMethod]
        public void Test_TwoParameters_NestedProperties()
        {
            var expr = "(x, y) => x.Teacher.Age > y.Teacher.Age"
                .ToExpression<Func<Student, Student, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(students[0], students[1])); // 30 > 45 = false
            Assert.IsTrue(compiled(students[1], students[0])); // 45 > 30 = true
        }

        [TestMethod]
        public void Test_ComplexNullHandling()
        {
            var expr = "(x, y) => (x ?? 0) + (y ?? 0)"
                .ToExpression<Func<int?, int?, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(15, compiled(10, 5));
            Assert.AreEqual(10, compiled(10, null));
            Assert.AreEqual(5, compiled(null, 5));
            Assert.AreEqual(0, compiled(null, null));
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Test_TwoParameters_BothZero()
        {
            var expr = "(x, y) => x + y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(0, compiled(0, 0));
        }

        [TestMethod]
        public void Test_TwoParameters_NegativeNumbers()
        {
            var expr = "(x, y) => x * y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(-20, compiled(-4, 5));
            Assert.AreEqual(20, compiled(-4, -5));
            Assert.AreEqual(-20, compiled(4, -5));
        }

        [TestMethod]
        public void Test_TwoParameters_LargeNumbers()
        {
            var expr = "(x, y) => x + y"
                .ToExpression<Func<int, int, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(2000000, compiled(1000000, 1000000));
        }

        [TestMethod]
        public void Test_BooleanParameters()
        {
            var expr = "(x, y) => x && y"
                .ToExpression<Func<bool, bool, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(true, true));
            Assert.IsFalse(compiled(true, false));
            Assert.IsFalse(compiled(false, true));
            Assert.IsFalse(compiled(false, false));
        }

        [TestMethod]
        public void Test_BooleanParameters_Or()
        {
            var expr = "(x, y) => x || y"
                .ToExpression<Func<bool, bool, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(true, true));
            Assert.IsTrue(compiled(true, false));
            Assert.IsTrue(compiled(false, true));
            Assert.IsFalse(compiled(false, false));
        }

        [TestMethod]
        public void Test_BooleanParameters_Xor()
        {
            var expr = "(x, y) => x ^ y"
                .ToExpression<Func<bool, bool, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(true, true));
            Assert.IsTrue(compiled(true, false));
            Assert.IsTrue(compiled(false, true));
            Assert.IsFalse(compiled(false, false));
        }

        #endregion
    }
}
