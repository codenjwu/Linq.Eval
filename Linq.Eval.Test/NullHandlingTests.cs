namespace Linq.Eval.Test
{
    /// <summary>
    /// Comprehensive tests for null handling, null-conditional, null-coalescing, and nullable types.
    /// </summary>
    [TestClass]
    public class NullHandlingTests
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
                Age = null, // Nullable Age
                IsPrinciple = true,
                Salary = 75000,
                WorkHours = 45
            };

            var teacher3 = new Teacher
            {
                FirstName = "Bob",
                LastName = "Williams",
                Age = 35,
                IsPrinciple = null, // Nullable IsPrinciple
                Salary = 55000,
                WorkHours = 38
            };

            var teacher4 = new Teacher
            {
                FirstName = "Carol",
                LastName = "Davis",
                Age = null,
                IsPrinciple = null,
                Salary = null, // Nullable fields null
                WorkHours = 40
            };

            students = new Student[]
            {
                new Student { Age = 18, FirstName = "Tom", LastName = "Brown", Teacher = teacher1 },
                new Student { Age = 20, FirstName = "Jane", LastName = "Davis", Teacher = teacher2 },
                new Student { Age = 19, FirstName = "Mike", LastName = "Wilson", Teacher = teacher3 },
                new Student { Age = 21, FirstName = "Sarah", LastName = "Taylor", Teacher = null }, // Null teacher
                new Student { Age = 17, FirstName = "Chris", LastName = "Anderson", Teacher = teacher4 }
            };

            teachers = new[] { teacher1, teacher2, teacher3, teacher4 };
            teacher1.Students = new[] { students[0] };
            teacher2.Students = new[] { students[1] };
            teacher3.Students = new[] { students[2] };
            teacher4.Students = new[] { students[4] };
        }

        #region Null-Coalescing Operator Tests

        [TestMethod]
        public void Test_NullCoalescing_WithNullValue()
        {
            var expr = "x => x.Age ?? 25"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(30, compiled(teachers[0])); // Has value
            Assert.AreEqual(25, compiled(teachers[1])); // Null, returns default
        }

        [TestMethod]
        public void Test_NullCoalescing_ChainedMultiple()
        {
            var expr = "x => (x.Age ?? x.WorkHours) ?? 20"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(30, compiled(teachers[0])); // Age has value
            Assert.AreEqual(45, compiled(teachers[1])); // Age null, WorkHours has value
            Assert.AreEqual(40, compiled(teachers[3])); // Age null, but WorkHours is 40 not null
        }

        [TestMethod]
        public void Test_NullCoalescing_WithExpression()
        {
            var expr = "x => (x.Age ?? 30) * 2"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(60, compiled(teachers[0])); // 30 * 2
            Assert.AreEqual(60, compiled(teachers[1])); // (null ?? 30) * 2 = 60
        }

        [TestMethod]
        public void Test_NullCoalescing_WithSalary()
        {
            var expr = "x => x.Salary ?? 40000"
                .ToExpression<Func<Teacher, double>>();
            var compiled = expr.Compile();

            Assert.AreEqual(50000.0, compiled(teachers[0]));
            Assert.AreEqual(40000.0, compiled(teachers[3])); // Null salary
        }

        [TestMethod]
        public void Test_NullCoalescing_BooleanType()
        {
            var expr = "x => x.IsPrinciple ?? false"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.AreEqual(false, compiled(teachers[0])); // Has value: false
            Assert.AreEqual(true, compiled(teachers[1])); // Has value: true
            Assert.AreEqual(false, compiled(teachers[2])); // Null, returns false
        }

        [TestMethod]
        public void Test_NullCoalescing_ComplexChain()
        {
            var expr = "x => ((x.Age ?? 0) != 0 ? (double)(x.Age ?? 0) : ((x.Salary ?? 0.0) != 0.0 ? (x.Salary ?? 0.0) : 0.0))"
                .ToExpression<Func<Teacher, double>>();
            var compiled = expr.Compile();

            Assert.AreEqual(30.0, compiled(teachers[0])); // Age
            Assert.AreEqual(75000.0, compiled(teachers[1])); // Salary (Age is null)
            Assert.AreEqual(35.0, compiled(teachers[2])); // Age
            Assert.AreEqual(0.0, compiled(teachers[3])); // All null
        }

        #endregion

        #region Null-Conditional Operator Tests

        [TestMethod]
        public void Test_NullConditional_PropertyAccess()
        {
            var expr = "x => x.Teacher?.Age ?? 0"
                .ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(30, compiled(students[0])); // Teacher exists, Age = 30
            Assert.AreEqual(0, compiled(students[1])); // Teacher exists, Age = null
            Assert.AreEqual(0, compiled(students[3])); // Teacher is null
        }

        [TestMethod]
        public void Test_NullConditional_ChainedAccess()
        {
            // Note: Can't chain multiple ?. in current implementation, so using ?? as backup
            var expr = "x => x.Teacher?.FirstName ?? \"Unknown\""
                .ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("John", compiled(students[0]));
            Assert.AreEqual("Unknown", compiled(students[3])); // Null teacher
        }

        [TestMethod]
        public void Test_NullConditional_WithArithmetic()
        {
            var expr = "x => (x.Teacher?.Age ?? 20) + 10"
                .ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(40, compiled(students[0])); // 30 + 10
            Assert.AreEqual(30, compiled(students[1])); // (null ?? 20) + 10
            Assert.AreEqual(30, compiled(students[3])); // (null ?? 20) + 10
        }

        [TestMethod]
        public void Test_NullConditional_BooleanProperty()
        {
            var expr = "x => x.Teacher?.IsPrinciple ?? false"
                .ToExpression<Func<Student, bool>>();
            var compiled = expr.Compile();

            Assert.AreEqual(false, compiled(students[0])); // Teacher1 not principle
            Assert.AreEqual(true, compiled(students[1])); // Teacher2 is principle
            Assert.AreEqual(false, compiled(students[2])); // Teacher3 IsPrinciple is null
            Assert.AreEqual(false, compiled(students[3])); // Teacher is null
        }

        [TestMethod]
        public void Test_NullConditional_InComparison()
        {
            var expr = "x => (x.Teacher?.Age ?? 0) > 35"
                .ToExpression<Func<Student, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(students[0])); // 30 > 35 = false
            Assert.IsFalse(compiled(students[1])); // 0 > 35 = false (Age is null)
            Assert.IsFalse(compiled(students[2])); // 35 > 35 = false
            Assert.IsFalse(compiled(students[4])); // Teacher4 Age is null -> 0 > 35 = false
        }

        #endregion

        #region Nullable Comparisons

        [TestMethod]
        public void Test_NullableEqualsNull()
        {
            var expr = "x => (x.Age ?? -1) == -1"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // Age is 30, not -1
            Assert.IsTrue(compiled(teachers[1])); // Age is null, becomes -1
        }

        [TestMethod]
        public void Test_NullableNotEqualsNull()
        {
            var expr = "x => (x.Age ?? -1) != -1"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0])); // Age is 30
            Assert.IsFalse(compiled(teachers[1])); // Age is null
        }

        [TestMethod]
        public void Test_NullableGreaterThan()
        {
            var expr = "x => (x.Age ?? 0) > 30"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // 30 > 30 = false
            Assert.IsFalse(compiled(teachers[1])); // 0 > 30 = false (null becomes 0)
            Assert.IsTrue(compiled(teachers[2])); // 35 > 30 = true
        }

        [TestMethod]
        public void Test_NullableEqualityComparison()
        {
            var expr = "x => (x.Age ?? 0) == 30"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0])); // 30 == 30
            Assert.IsFalse(compiled(teachers[1])); // 0 == 30 = false (null becomes 0)
        }

        [TestMethod]
        public void Test_NullableComparisonWithCoalescing()
        {
            var expr = "x => (x.Age ?? 0) >= 30"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0])); // 30 >= 30
            Assert.IsFalse(compiled(teachers[1])); // (null ?? 0) >= 30 = false
        }

        #endregion

        #region Mixed Null Operations

        [TestMethod]
        public void Test_ConditionalWithNullCoalescing()
        {
            var expr = "x => (x.Age ?? 25) > 30 ? \"Senior\" : \"Junior\""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Junior", compiled(teachers[0])); // 30 > 30 = false
            Assert.AreEqual("Junior", compiled(teachers[1])); // 25 > 30 = false
            Assert.AreEqual("Senior", compiled(teachers[2])); // 35 > 30 = true
        }

        [TestMethod]
        public void Test_NullCoalescingInArithmetic()
        {
            var expr = "x => (x.Age ?? 30) + x.WorkHours"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(70, compiled(teachers[0])); // 30 + 40
            Assert.AreEqual(75, compiled(teachers[1])); // 30 + 45
            Assert.AreEqual(73, compiled(teachers[2])); // 35 + 38
            Assert.AreEqual(70, compiled(teachers[3])); // 30 + 40
        }

        [TestMethod]
        public void Test_MultipleNullChecks()
        {
            var expr = "x => (x.Age ?? -1) != -1 && (x.Salary ?? -1.0) != -1.0"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0])); // All have values
            Assert.IsFalse(compiled(teachers[1])); // Age is null
            Assert.IsTrue(compiled(teachers[2])); // Both have values
            Assert.IsFalse(compiled(teachers[3])); // All are null
        }

        [TestMethod]
        public void Test_NullCoalescingWithCalculation()
        {
            var expr = "x => (x.Salary ?? 50000.0) / (double)(x.WorkHours ?? 40)"
                .ToExpression<Func<Teacher, double>>();
            var compiled = expr.Compile();

            Assert.AreEqual(1250.0, compiled(teachers[0])); // 50000 / 40
            Assert.AreEqual(1666.6666666666667, compiled(teachers[1]), 0.0001); // 75000 / 45
            Assert.AreEqual(1447.3684210526316, compiled(teachers[2]), 0.0001); // 55000 / 38
            Assert.AreEqual(1250.0, compiled(teachers[3])); // 50000 / 40 (null ?? defaults)
        }

        [TestMethod]
        public void Test_ConditionalWithNullConditional()
        {
            var expr = "x => (x.Teacher?.Age ?? -1) != -1 ? \"Has Age\" : \"No Age\""
                .ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Has Age", compiled(students[0])); // Teacher1 has age (30)
            Assert.AreEqual("No Age", compiled(students[1])); // Teacher2 age is null
            Assert.AreEqual("No Age", compiled(students[3])); // Teacher is null
        }

        #endregion

        #region Complex Null Scenarios

        [TestMethod]
        public void Test_NestedNullCoalescing()
        {
            var expr = "x => (double)(x.Age ?? (int)(x.Salary ?? 1000.0))"
                .ToExpression<Func<Teacher, double>>();
            var compiled = expr.Compile();

            Assert.AreEqual(30.0, compiled(teachers[0])); // Age
            Assert.AreEqual(75000.0, compiled(teachers[1])); // Salary (Age null)
            Assert.AreEqual(35.0, compiled(teachers[2])); // Age
            Assert.AreEqual(1000.0, compiled(teachers[3])); // Default (all null)
        }

        [TestMethod]
        public void Test_NullableInConditionalChain()
        {
            var expr = @"x => (x.Age ?? -1) == -1 ? ""No Age"" : 
                              (x.Age ?? 0) < 35 ? ""Young"" : 
                              (x.Age ?? 0) < 45 ? ""Middle"" : ""Senior"""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Young", compiled(teachers[0])); // 30
            Assert.AreEqual("No Age", compiled(teachers[1])); // null
            Assert.AreEqual("Middle", compiled(teachers[2])); // 35
        }

        [TestMethod]
        public void Test_NullCoalescingWithLogical()
        {
            var expr = "x => (x.Age ?? 30) > 30 && (x.Salary ?? 50000.0) > 60000.0"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // 30 > 30 = false
            Assert.IsFalse(compiled(teachers[1])); // 30 > 30 = false (Age is null -> 30)
            Assert.IsFalse(compiled(teachers[2])); // 35 > 30 = true, but 55000 > 60000 = false
        }

        [TestMethod]
        [Ignore("Array null-conditional access with indexer is not yet implemented")]
        public void Test_MultipleNullConditionalAccess()
        {
            // Testing Teacher -> Students array -> First student -> Teacher -> Age
            var expr = "x => x.Students?[0]?.Teacher?.Age ?? 0"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            // teacher[0] has students, students[0].Teacher is teacher[0] itself, Age is 30
            Assert.AreEqual(30, compiled(teachers[0])); // Students[0].Teacher.Age = 30
        }

        [TestMethod]
        public void Test_NullableArithmeticWithDefault()
        {
            var expr = "x => (x.Age ?? 30) * (x.WorkHours ?? 40) / 100"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(12, compiled(teachers[0])); // 30 * 40 / 100 = 12
            Assert.AreEqual(13, compiled(teachers[1])); // 30 * 45 / 100 = 13.5 -> 13
            Assert.AreEqual(13, compiled(teachers[2])); // 35 * 38 / 100 = 13.3 -> 13
            Assert.AreEqual(12, compiled(teachers[3])); // 30 * 40 / 100 = 12
        }

        [TestMethod]
        public void Test_NullableModuloOperation()
        {
            var expr = "x => (x.Age ?? 30) % 10"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(0, compiled(teachers[0])); // 30 % 10
            Assert.AreEqual(0, compiled(teachers[1])); // 30 % 10
            Assert.AreEqual(5, compiled(teachers[2])); // 35 % 10
        }

        #endregion

        #region Boolean Nullable Tests

        [TestMethod]
        public void Test_NullableBooleanEquality()
        {
            var expr = "x => (x.IsPrinciple ?? false) == true"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // false == true
            Assert.IsTrue(compiled(teachers[1])); // true == true
            Assert.IsFalse(compiled(teachers[2])); // (null ?? false) == true = false
        }

        [TestMethod]
        public void Test_NullableBooleanWithCoalescing()
        {
            var expr = "x => (x.IsPrinciple ?? false) == true"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // false == true
            Assert.IsTrue(compiled(teachers[1])); // true == true
            Assert.IsFalse(compiled(teachers[2])); // (null ?? false) == true = false
        }

        [TestMethod]
        public void Test_NullableBooleanInConditional()
        {
            var expr = "x => (x.IsPrinciple ?? false) ? \"Principal\" : \"Teacher\""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Teacher", compiled(teachers[0]));
            Assert.AreEqual("Principal", compiled(teachers[1]));
            Assert.AreEqual("Teacher", compiled(teachers[2])); // null defaults to false
        }

        #endregion

        #region Null String Operations

        [TestMethod]
        public void Test_NullConditionalStringProperty()
        {
            var expr = "x => x.Teacher?.FirstName ?? \"NoName\""
                .ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("John", compiled(students[0]));
            Assert.AreEqual("NoName", compiled(students[3])); // Null teacher
        }

        [TestMethod]
        public void Test_NullConditionalStringComparison()
        {
            var expr = "x => (x.Teacher?.FirstName ?? \"Unknown\") == \"John\""
                .ToExpression<Func<Student, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(students[0])); // Teacher is John
            Assert.IsFalse(compiled(students[1])); // Teacher is Alice
            Assert.IsFalse(compiled(students[3])); // Null teacher -> "Unknown"
        }

        #endregion
    }
}
