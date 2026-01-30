namespace Linq.Eval.Test
{
    /// <summary>
    /// Tests for string operations, comparisons, and manipulations.
    /// </summary>
    [TestClass]
    public class StringOperationTests
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

            var teacher4 = new Teacher
            {
                FirstName = "",
                LastName = "Davis",
                Age = 50,
                IsPrinciple = true,
                Salary = 80000,
                WorkHours = 42
            };

            students = new Student[]
            {
                new Student { Age = 18, FirstName = "Tom", LastName = "Brown", Teacher = teacher1 },
                new Student { Age = 20, FirstName = "Jane", LastName = "Davis", Teacher = teacher2 },
                new Student { Age = 19, FirstName = "Mike", LastName = "Wilson", Teacher = teacher3 },
                new Student { Age = 21, FirstName = "Sarah", LastName = "Taylor", Teacher = teacher4 },
                new Student { Age = 17, FirstName = "john", LastName = "anderson", Teacher = teacher1 }, // Lowercase
            };

            teachers = new[] { teacher1, teacher2, teacher3, teacher4 };
            teacher1.Students = new[] { students[0], students[4] };
            teacher2.Students = new[] { students[1] };
            teacher3.Students = new[] { students[2] };
            teacher4.Students = new[] { students[3] };
        }

        #region String Equality Tests

        [TestMethod]
        public void Test_StringEquality()
        {
            var expr = "x => x.FirstName == \"John\" && (x.Age ?? 0) > 25"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0]));
            Assert.IsFalse(compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_StringInequality()
        {
            var expr = "x => x.FirstName != \"John\""
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0]));
            Assert.IsTrue(compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_StringEqualityWithEmptyString()
        {
            var expr = "x => x.FirstName == \"\""
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0]));
            Assert.IsTrue(compiled(teachers[3])); // Empty FirstName
        }

        [TestMethod]
        public void Test_StringComparisonInConditional()
        {
            var expr = "x => x.FirstName == \"John\" ? \"Found\" : \"NotFound\""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Found", compiled(teachers[0]));
            Assert.AreEqual("NotFound", compiled(teachers[1]));
        }

        #endregion

        #region String Logical Operations

        [TestMethod]
        public void Test_MultipleStringComparisons()
        {
            var expr = "x => x.FirstName == \"John\" || x.FirstName == \"Alice\""
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0])); // John
            Assert.IsTrue(compiled(teachers[1])); // Alice
            Assert.IsFalse(compiled(teachers[2])); // Bob
        }

        [TestMethod]
        public void Test_StringAndNumericCondition()
        {
            var expr = "x => x.FirstName == \"John\" && (x.Age ?? 0) > 25"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0])); // John, Age 30
        }

        [TestMethod]
        public void Test_ComplexStringLogic()
        {
            var expr = "x => (x.FirstName == \"John\" || x.FirstName == \"Alice\") && (x.Age ?? 0) > 30"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // John but age is 30, not > 30
            Assert.IsTrue(compiled(teachers[1])); // Alice and age 45 > 30
        }

        #endregion

        #region String with Null Handling

        [TestMethod]
        public void Test_StringPropertyWithNullConditional()
        {
            var expr = "x => x.Teacher?.FirstName ?? \"Unknown\""
                .ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("John", compiled(students[0]));
            Assert.AreEqual("Alice", compiled(students[1]));
        }

        [TestMethod]
        public void Test_StringComparisonWithNullConditional()
        {
            var expr = "x => (x.Teacher?.FirstName ?? \"None\") == \"John\""
                .ToExpression<Func<Student, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(students[0]));
            Assert.IsFalse(compiled(students[1]));
        }

        [TestMethod]
        public void Test_EmptyStringHandling()
        {
            var expr = "x => x.FirstName == \"\" ? \"Empty\" : x.FirstName"
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("John", compiled(teachers[0]));
            Assert.AreEqual("Empty", compiled(teachers[3]));
        }

        #endregion

        #region Nested String Operations

        [TestMethod]
        public void Test_NestedPropertyStringAccess()
        {
            var expr = "x => x.Teacher.FirstName"
                .ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("John", compiled(students[0]));
            Assert.AreEqual("Alice", compiled(students[1]));
            Assert.AreEqual("Bob", compiled(students[2]));
        }

        [TestMethod]
        public void Test_NestedStringComparison()
        {
            var expr = "x => x.Teacher.FirstName == \"John\""
                .ToExpression<Func<Student, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(students[0]));
            Assert.IsFalse(compiled(students[1]));
        }

        [TestMethod]
        public void Test_ConditionalWithNestedString()
        {
            var expr = "x => x.Teacher.FirstName == \"Alice\" ? x.Age + 5 : x.Age"
                .ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(18, compiled(students[0])); // Teacher is John
            Assert.AreEqual(25, compiled(students[1])); // Teacher is Alice: 20 + 5
        }

        #endregion

        #region Complex String Scenarios

        [TestMethod]
        public void Test_MultipleStringPropertiesComparison()
        {
            var expr = "x => x.FirstName == \"John\" && x.LastName == \"Smith\""
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0]));
            Assert.IsFalse(compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_StringInComplexConditional()
        {
            var expr = @"x => x.FirstName == ""John"" ? ""J"" : 
                              x.FirstName == ""Alice"" ? ""A"" : 
                              x.FirstName == ""Bob"" ? ""B"" : ""Other"""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("J", compiled(teachers[0]));
            Assert.AreEqual("A", compiled(teachers[1]));
            Assert.AreEqual("B", compiled(teachers[2]));
            Assert.AreEqual("Other", compiled(teachers[3]));
        }

        [TestMethod]
        public void Test_StringWithArithmeticResult()
        {
            var expr = "x => x.FirstName == \"John\" ? (x.Age ?? 0) * 2 : (x.Age ?? 0)"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(60, compiled(teachers[0])); // 30 * 2
            Assert.AreEqual(45, compiled(teachers[1])); // 45
        }

        [TestMethod]
        public void Test_NestedStringConditional()
        {
            var expr = "x => x.Teacher.FirstName == \"John\" ? (x.Age > 18 ? \"Adult\" : \"Teen\") : \"Other\""
                .ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Teen", compiled(students[0])); // Teacher John, Age 18 (18 > 18 = false)
            Assert.AreEqual("Other", compiled(students[1])); // Teacher Alice
            Assert.AreEqual("Teen", compiled(students[4])); // Teacher John, Age 17 (17 > 18 = false)
        }

        #endregion

        #region String Return Values

        [TestMethod]
        public void Test_ReturnDifferentStrings()
        {
            var expr = "x => (x.Age ?? 0) > 40 ? x.FirstName : x.LastName"
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Smith", compiled(teachers[0])); // Age 30, returns LastName
            Assert.AreEqual("Alice", compiled(teachers[1])); // Age 45, returns FirstName
        }

        [TestMethod]
        public void Test_StringLiteralReturn()
        {
            var expr = "x => x.IsPrinciple ? \"Principal\" : \"Teacher\""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Teacher", compiled(teachers[0]));
            Assert.AreEqual("Principal", compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_ComplexStringReturn()
        {
            var expr = "x => (x.Age ?? 0) > 40 ? ((x.IsPrinciple ?? false) ? \"Senior Principal\" : \"Senior Teacher\") : \"Junior\""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Junior", compiled(teachers[0])); // Age 30
            Assert.AreEqual("Senior Principal", compiled(teachers[1])); // Age 45, IsPrinciple true
            Assert.AreEqual("Junior", compiled(teachers[2])); // Age 35
            Assert.AreEqual("Senior Principal", compiled(teachers[3])); // Age 50, IsPrinciple true
        }

        #endregion

        #region String Special Cases

        [TestMethod]
        public void Test_StringWithSpecialCharacters()
        {
            var expr = "x => x.FirstName == \"John\""
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0]));
        }

        [TestMethod]
        public void Test_MultipleStringConditionsChained()
        {
            var expr = "x => x.FirstName != \"\" && x.LastName != \"\" && x.FirstName == \"John\""
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0]));
            Assert.IsFalse(compiled(teachers[3])); // Empty FirstName
        }

        [TestMethod]
        public void Test_StringComparisonWithNumerics()
        {
            var expr = "x => (x.FirstName == \"John\" ? 100 : 50) + (x.Age ?? 0)"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(130, compiled(teachers[0])); // 100 + 30
            Assert.AreEqual(95, compiled(teachers[1])); // 50 + 45
        }

        #endregion

        #region Case Sensitivity Tests

        [TestMethod]
        public void Test_CaseSensitiveComparison()
        {
            var expr = "x => x.FirstName == \"john\""
                .ToExpression<Func<Student, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(students[0])); // "Tom"
            Assert.IsTrue(compiled(students[4])); // "john" lowercase
        }

        [TestMethod]
        public void Test_MixedCaseProperties()
        {
            var expr = "x => x.FirstName == \"Tom\" && x.LastName == \"Brown\""
                .ToExpression<Func<Student, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(students[0]));
            Assert.IsFalse(compiled(students[4])); // firstname is "john"
        }

        #endregion

        #region String with Other Types

        [TestMethod]
        public void Test_StringAndBooleanCombination()
        {
            var expr = "x => (x.IsPrinciple ?? false) && x.FirstName == \"Alice\""
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // Not principle
            Assert.IsTrue(compiled(teachers[1])); // Both true
        }

        [TestMethod]
        public void Test_StringInComplexExpression()
        {
            var expr = "x => (x.FirstName == \"John\" ? 1 : 0) + (x.LastName == \"Smith\" ? 1 : 0)"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(2, compiled(teachers[0])); // Both match
            Assert.AreEqual(0, compiled(teachers[1])); // Neither match
        }

        [TestMethod]
        public void Test_NestedStringWithCalculations()
        {
            var expr = "x => x.Teacher.FirstName == \"John\" ? x.Age * 2 : x.Age + 10"
                .ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(36, compiled(students[0])); // Teacher is John: 18 * 2
            Assert.AreEqual(30, compiled(students[1])); // Teacher is Alice: 20 + 10
        }

        #endregion
    }
}
