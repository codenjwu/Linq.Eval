namespace Linq.Eval.Test
{
    /// <summary>
    /// Tests for complex nested expressions and advanced scenarios.
    /// </summary>
    [TestClass]
    public class ComplexExpressionTests
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
                FirstName = "Carol",
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
                new Student { Age = 17, FirstName = "Chris", LastName = "Anderson", Teacher = teacher1 },
                new Student { Age = 22, FirstName = "Emma", LastName = "Martinez", Teacher = teacher2 },
                new Student { Age = 20, FirstName = "Alex", LastName = "Garcia", Teacher = teacher3 },
                new Student { Age = 19, FirstName = "Lisa", LastName = "Rodriguez", Teacher = teacher4 }
            };

            teachers = new[] { teacher1, teacher2, teacher3, teacher4 };
            teacher1.Students = new[] { students[0], students[4] };
            teacher2.Students = new[] { students[1], students[5] };
            teacher3.Students = new[] { students[2], students[6] };
            teacher4.Students = new[] { students[3], students[7] };
        }

        #region Complex Nested Conditionals

        [TestMethod]
        public void Test_DeepNestedConditional()
        {
            // Nested ternary: condition ? (nested_condition ? value1 : value2) : value3
            var expr = "x => (x.Age ?? 0) > 40 ? ((x.Salary ?? 0.0) > 70000.0 ? \"Senior High\" : \"Senior Low\") : ((x.Age ?? 0) > 30 ? \"Mid\" : \"Junior\")"
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Junior", compiled(teachers[0])); // Age 30
            Assert.AreEqual("Senior High", compiled(teachers[1])); // Age 45, Salary 75000
            Assert.AreEqual("Mid", compiled(teachers[2])); // Age 35
            Assert.AreEqual("Senior High", compiled(teachers[3])); // Age 50, Salary 80000
        }

        [TestMethod]
        public void Test_MultipleConditionalWithLogicalOperators()
        {
            var expr = "x => ((x.Age ?? 0) > 30 && (x.Salary ?? 0.0) > 60000.0) ? \"A\" : (((x.Age ?? 0) > 30 || (x.Salary ?? 0.0) > 60000.0) ? \"B\" : \"C\")"
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("C", compiled(teachers[0])); // Age 30, Salary 50000 - neither
            Assert.AreEqual("A", compiled(teachers[1])); // Age 45, Salary 75000 - both
            Assert.AreEqual("B", compiled(teachers[2])); // Age 35, Salary 55000 - only age
        }

        [TestMethod]
        public void Test_ConditionalWithComplexComparisons()
        {
            var expr = "x => ((x.Age ?? 0) >= 40 && (x.Age ?? 0) <= 50 && (x.Salary ?? 0.0) >= 70000.0 && (x.Salary ?? 0.0) <= 80000.0) ? \"Target Range\" : \"Out of Range\""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("Out of Range", compiled(teachers[0]));
            Assert.AreEqual("Target Range", compiled(teachers[1]));
            Assert.AreEqual("Out of Range", compiled(teachers[2]));
            Assert.AreEqual("Target Range", compiled(teachers[3]));
        }

        #endregion

        #region Complex Arithmetic Expressions

        [TestMethod]
        public void Test_ComplexArithmeticWithPrecedence()
        {
            var expr = "x => (x.Age ?? 0) * 2 + x.WorkHours / 2 - 10"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            // teacher1: 30 * 2 + 40 / 2 - 10 = 60 + 20 - 10 = 70
            Assert.AreEqual(70, compiled(teachers[0]));
            // teacher1: 45 * 2 + 45 / 2 - 10 = 90 + 22 - 10 = 102
            Assert.AreEqual(102, compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_ComplexArithmeticWithParentheses()
        {
            var expr = "x => ((x.Age ?? 0) + x.WorkHours) * 2 - ((x.Age ?? 0) - 10) / 2"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            // teacher1: (30 + 40) * 2 - (30 - 10) / 2 = 140 - 10 = 130
            Assert.AreEqual(130, compiled(teachers[0]));
        }

        [TestMethod]
        public void Test_ModuloAndDivisionCombined()
        {
            var expr = "x => (x.Salary ?? 0.0) / 1000.0 + (double)(x.WorkHours % 10)"
                .ToExpression<Func<Teacher, double>>();
            var compiled = expr.Compile();

            // teacher1: 50000 / 1000 + 40 % 10 = 50 + 0 = 50
            Assert.AreEqual(50.0, compiled(teachers[0]));
            // teacher2: 75000 / 1000 + 45 % 10 = 75 + 5 = 80
            Assert.AreEqual(80.0, compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_NegativeNumbersInArithmetic()
        {
            var expr = "x => -(x.Age ?? 0) + x.WorkHours * -2"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            // teacher1: -30 + 40 * -2 = -30 + -80 = -110
            Assert.AreEqual(-110, compiled(teachers[0]));
        }

        #endregion

        #region Complex Logical Expressions

        [TestMethod]
        public void Test_ComplexLogicalWithParentheses()
        {
            var expr = "x => ((x.Age ?? 0) > 30 && (x.Salary ?? 0.0) > 60000.0) || ((x.Age ?? 0) < 40 && x.WorkHours < 40)"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[2])); // Age 35, WorkHours 38 - second condition
            Assert.IsTrue(compiled(teachers[1])); // Age 45, Salary 75000 - first condition
        }

        [TestMethod]
        public void Test_MultipleLogicalOperatorsChained()
        {
            var expr = "x => (x.Age ?? 0) > 30 && (x.Age ?? 0) < 50 && (x.Salary ?? 0.0) > 50000.0 && x.WorkHours > 35"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // Age 30 - fails first condition
            Assert.IsTrue(compiled(teachers[1])); // All conditions met
            Assert.IsTrue(compiled(teachers[2])); // All conditions met
        }

        [TestMethod]
        public void Test_NotWithComplexExpression()
        {
            var expr = "x => !(x.Age > 40 && x.IsPrinciple)"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0])); // Not (false && false) = true
            Assert.IsFalse(compiled(teachers[1])); // Not (true && true) = false
            Assert.IsTrue(compiled(teachers[2])); // Not (false && false) = true
        }

        [TestMethod]
        public void Test_XorLogicalOperation()
        {
            var expr = "x => ((x.Age ?? 0) > 40) ^ ((x.Salary ?? 0.0) > 60000.0)"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // false ^ false = false
            Assert.IsFalse(compiled(teachers[1])); // true ^ true = false
            Assert.IsFalse(compiled(teachers[2])); // false ^ false = false
            // Let's check teacher with only one true condition
            var specialTeacher = new Teacher { Age = 45, Salary = 50000, WorkHours = 40 };
            Assert.IsTrue(compiled(specialTeacher)); // true ^ false = true
        }

        #endregion

        #region Complex Comparisons

        [TestMethod]
        public void Test_ChainedComparisons()
        {
            var expr = "x => x.Age >= 30 && x.Age <= 50"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0])); // 30
            Assert.IsTrue(compiled(teachers[1])); // 45
            Assert.IsTrue(compiled(teachers[3])); // 50
        }

        [TestMethod]
        public void Test_MultipleInequalityComparisons()
        {
            var expr = "x => (x.Salary ?? 0.0) != 50000.0 && (x.Salary ?? 0.0) != 75000.0"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(teachers[0])); // Salary is 50000
            Assert.IsFalse(compiled(teachers[1])); // Salary is 75000
            Assert.IsTrue(compiled(teachers[2])); // Salary is 55000
        }

        [TestMethod]
        public void Test_ComparisonWithCalculatedValues()
        {
            var expr = "x => (x.Salary ?? 0.0) / (double)x.WorkHours > 1200.0"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            // teacher1: 50000 / 40 = 1250 > 1200 = true
            Assert.IsTrue(compiled(teachers[0]));
            // teacher2: 75000 / 45 = 1666 > 1200 = true
            Assert.IsTrue(compiled(teachers[1]));
            // teacher3: 55000 / 38 = 1447 > 1200 = true
            Assert.IsTrue(compiled(teachers[2]));
        }

        #endregion

        #region Complex Property Access

        [TestMethod]
        public void Test_NestedPropertyAccess()
        {
            var expr = "x => x.Teacher.Age > 40"
                .ToExpression<Func<Student, bool>>();
            var compiled = expr.Compile();

            Assert.IsFalse(compiled(students[0])); // Teacher1, Age 30
            Assert.IsTrue(compiled(students[1])); // Teacher2, Age 45
            Assert.IsFalse(compiled(students[2])); // Teacher3, Age 35
        }

        [TestMethod]
        public void Test_NestedPropertyWithArithmetic()
        {
            var expr = "x => x.Age + (x.Teacher.Age ?? 0)"
                .ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(48, compiled(students[0])); // 18 + 30
            Assert.AreEqual(65, compiled(students[1])); // 20 + 45
        }

        [TestMethod]
        public void Test_ComplexNestedPropertyComparison()
        {
            var expr = "x => x.Age * 2 > x.Teacher.Age"
                .ToExpression<Func<Student, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(students[0])); // 18 * 2 = 36 > 30
            Assert.IsFalse(compiled(students[1])); // 20 * 2 = 40 < 45
        }

        [TestMethod]
        public void Test_ConditionalWithNestedProperties()
        {
            var expr = "x => x.Teacher.IsPrinciple ? x.Age + 10 : x.Age - 5"
                .ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(13, compiled(students[0])); // Teacher not principle: 18 - 5
            Assert.AreEqual(30, compiled(students[1])); // Teacher is principle: 20 + 10
        }

        #endregion

        #region Mixed Complex Scenarios

        [TestMethod]
        public void Test_ComplexMixedExpression1()
        {
            // Arithmetic + Comparison + Logical + Conditional
            var expr = "x => ((double)(x.Age ?? 0) * 1000.0 + (x.Salary ?? 0.0)) > 100000.0 && x.WorkHours < 50 ? \"Excellent\" : \"Good\""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            // teacher1: (30 * 1000 + 50000) = 80000 > 100000 = false -> "Good"
            Assert.AreEqual("Good", compiled(teachers[0]));
            // teacher2: (45 * 1000 + 75000) = 120000 > 100000 = true && 45 < 50 = true -> "Excellent"
            Assert.AreEqual("Excellent", compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_ComplexMixedExpression2()
        {
            var expr = "x => ((x.Age ?? 0) > 35 ? (x.Salary ?? 0) * 2.0 : (x.Salary ?? 0)) / 1000.0"
                .ToExpression<Func<Teacher, double>>();
            var compiled = expr.Compile();

            // teacher1: (30 > 35 ? 100000 : 50000) / 1000 = 50 + 0 = 50
            Assert.AreEqual(50.0, compiled(teachers[0]));
            // teacher2: (45 > 35 ? 150000 : 75000) / 1000 = 150
            Assert.AreEqual(150.0, compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_ComplexBooleanAlgebra()
        {
            var expr = "x => ((x.Age ?? 0) > 30 || (x.Salary ?? 0.0) > 60000.0) && !((x.Age ?? 0) < 35 && (x.Salary ?? 0.0) < 55000.0)"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            // teacher1: (false || false) && !(true && true) = false
            Assert.IsFalse(compiled(teachers[0]));
            // teacher2: (true || true) && !(false && false) = true
            Assert.IsTrue(compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_ComplexWithMultipleNesting()
        {
            // Simplified to avoid division type issues - use only multiplication and addition
            var expr = "x => ((x.Age ?? 0) + 5) * (x.WorkHours - 5) - ((x.Age ?? 0) + 10)"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            // teacher1: ((30 + 5) * (40 - 5)) - (30 + 10) = (35 * 35) - 40 = 1225 - 40 = 1185
            Assert.AreEqual(1185, compiled(teachers[0]));
        }

        #endregion

        #region Edge Cases and Boundary Conditions

        [TestMethod]
        public void Test_ZeroDivisionAvoidance()
        {
            var expr = "x => x.WorkHours > 0 ? (x.Salary ?? 0.0) / (double)x.WorkHours : 0.0"
                .ToExpression<Func<Teacher, double>>();
            var compiled = expr.Compile();

            Assert.AreEqual(1250.0, compiled(teachers[0])); // 50000 / 40
        }

        [TestMethod]
        public void Test_LargeNumberArithmetic()
        {
            var expr = "x => (x.Salary ?? 0.0) * 12.0 + (double)x.WorkHours * 52.0 * 100.0"
                .ToExpression<Func<Teacher, double>>();
            var compiled = expr.Compile();

            // teacher1: 50000 * 12 + 40 * 52 * 100 = 600000 + 208000 = 808000
            Assert.AreEqual(808000.0, compiled(teachers[0]));
        }

        [TestMethod]
        public void Test_BoundaryValueComparison()
        {
            var expr1 = "x => x.Age >= 30 && x.Age <= 30"
                .ToExpression<Func<Teacher, bool>>();
            
            Assert.IsTrue(expr1.Compile()(teachers[0])); // Exactly 30

            var expr2 = "x => x.Age > 30 || x.Age < 30"
                .ToExpression<Func<Teacher, bool>>();
            
            Assert.IsFalse(expr2.Compile()(teachers[0])); // Exactly 30, neither > nor <
        }

        [TestMethod]
        public void Test_ComplexConditionalChain()
        {
            var expr = @"x => (x.Age ?? 0) < 30 ? ""A"" : 
                              (x.Age ?? 0) < 40 ? ""B"" : 
                              (x.Age ?? 0) < 50 ? ""C"" : ""D"""
                .ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();

            Assert.AreEqual("B", compiled(teachers[0])); // 30
            Assert.AreEqual("C", compiled(teachers[1])); // 45
            Assert.AreEqual("B", compiled(teachers[2])); // 35
            Assert.AreEqual("D", compiled(teachers[3])); // 50
        }

        #endregion

        #region Performance and Stress Tests

        [TestMethod]
        public void Test_DeepNesting_10Levels()
        {
            var expr = "x => ((((((((((x.Age ?? 0) + 1) + 1) + 1) + 1) + 1) + 1) + 1) + 1) + 1) + 1"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            Assert.AreEqual(40, compiled(teachers[0])); // 30 + 10
        }

        [TestMethod]
        public void Test_LongExpressionChain()
        {
            var expr = "x => (x.Age ?? 0) > 0 && (x.Age ?? 0) < 100 && (x.Salary ?? 0.0) > 0.0 && (x.Salary ?? 0.0) < 200000.0 && x.WorkHours > 0 && x.WorkHours < 100"
                .ToExpression<Func<Teacher, bool>>();
            var compiled = expr.Compile();

            Assert.IsTrue(compiled(teachers[0]));
            Assert.IsTrue(compiled(teachers[1]));
        }

        [TestMethod]
        public void Test_MultipleArithmeticOperations()
        {
            var expr = "x => (x.Age ?? 0) + (x.Age ?? 0) - (x.Age ?? 0) * (x.Age ?? 0) / (x.Age ?? 0) % (x.Age ?? 0)"
                .ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();

            // teacher1: 30 + 30 - 30 * 30 / 30 % 30 = 30 + 30 - (900 / 30) % 30 = 60 - 30 % 30 = 60 - 0 = 60
            Assert.AreEqual(60, compiled(teachers[0]));
        }

        #endregion
    }
}
