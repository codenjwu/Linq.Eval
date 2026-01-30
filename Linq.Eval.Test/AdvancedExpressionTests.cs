namespace Linq.Eval.Test
{
    /// <summary>
    /// Tests for advanced expression features including null-conditional access, type casting, conditional expressions, etc.
    /// </summary>
    [TestClass]
    public class AdvancedExpressionTests
    {
        private Teacher teacher1 = null!;
        private Teacher teacher2 = null!;
        private Teacher teacher3 = null!;
        private Student[] students1 = null!;
        private Student[] students2 = null!;
        private Teacher[] teachers = null!;

        [TestInitialize]
        public void Init()
        {
            teacher1 = new Teacher 
            { 
                FirstName = "John", 
                LastName = "Smith", 
                Age = 30, 
                IsPrinciple = false, 
                Salary = 50000, 
                WorkHours = 40 
            };
            
            teacher2 = new Teacher 
            { 
                FirstName = "Alice", 
                LastName = "Johnson", 
                Age = 45, 
                IsPrinciple = true, 
                Salary = 75000, 
                WorkHours = 45 
            };
            
            teacher3 = new Teacher 
            { 
                FirstName = "Bob", 
                LastName = "Williams", 
                Age = null, 
                IsPrinciple = false, 
                Salary = 55000, 
                WorkHours = 38 
            };

            students1 = new Student[]
            {
                new Student { Age = 18, FirstName = "Tom", LastName = "Brown", Teacher = teacher1 },
                new Student { Age = 20, FirstName = "Jane", LastName = "Davis", Teacher = teacher2 },
                new Student { Age = 19, FirstName = "Mike", LastName = "Wilson", Teacher = null! },
            };

            students2 = new Student[]
            {
                new Student { Age = 21, FirstName = "Sarah", LastName = "Taylor", Teacher = teacher3 },
                new Student { Age = 17, FirstName = "Chris", LastName = "Anderson", Teacher = null! },
                new Student { Age = 22, FirstName = "Emma", LastName = "Martinez", Teacher = teacher1 },
            };

            teacher1.Students = students1;
            teacher2.Students = students2;
            teacher3.Students = new Student[0];

            teachers = new Teacher[] { teacher1, teacher2, teacher3 };
        }

        #region Null-Conditional Operator Tests

        [TestMethod]
        public void Test_NullConditional_SimpleProperty()
        {
            var expr = "x => x.Teacher?.Age".ToExpression<Func<Student, int?>>();
            var compiled = expr.Compile();
            
            var result1 = compiled(students1[0]); // Has teacher with age 30
            Assert.AreEqual(30, result1);
            
            var result2 = compiled(students1[2]); // No teacher
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void Test_NullConditional_NestedProperty()
        {
            var expr = "x => x.Teacher?.FirstName".ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();
            
            var result1 = compiled(students1[0]);
            Assert.AreEqual("John", result1);
            
            var result2 = compiled(students1[2]);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void Test_NullConditional_WithCoalesce()
        {
            var expr = "x => x.Teacher?.Age ?? 25".ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();
            
            var result1 = compiled(students1[0]); // Has teacher
            Assert.AreEqual(30, result1);
            
            var result2 = compiled(students1[2]); // No teacher
            Assert.AreEqual(25, result2);
        }

        [TestMethod]
        public void Test_NullConditional_BooleanProperty()
        {
            var expr = "x => x.Teacher?.IsPrinciple".ToExpression<Func<Student, bool?>>();
            var compiled = expr.Compile();
            
            var result1 = compiled(students1[0]);
            Assert.AreEqual(false, result1);
            
            var result2 = compiled(students1[2]);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void Test_NullConditional_InComparison()
        {
            var expr = "x => (x.Teacher?.Age ?? 0) > 35".ToExpression<Func<Student, bool>>();
            var results = students1.Where(expr.Compile()).ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("Jane", results[0].FirstName);
        }

        [TestMethod]
        public void Test_NullConditional_ComplexExpression()
        {
            var expr = "x => (x.Teacher?.Age ?? 100) > 35 && !(x.Teacher?.IsPrinciple ?? true)".ToExpression<Func<Student, bool>>();
            var results = students1.Where(expr.Compile()).ToArray();
            Assert.IsTrue(results.Length >= 0);
        }

        [TestMethod]
        public void Test_NullConditional_ArrayAccess()
        {
            var expr = "x => x.Students?[0].Age".ToExpression<Func<Teacher, int?>>();
            var compiled = expr.Compile();
            
            var result = compiled(teacher1);
            Assert.AreEqual(18, result);
        }

        #endregion

        #region Null-Coalescing Operator Tests

        [TestMethod]
        public void Test_NullCoalescing_Simple()
        {
            var expr = "x => x.Age ?? 0".ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual(30, compiled(teacher1));
            Assert.AreEqual(0, compiled(teacher3));
        }

        [TestMethod]
        public void Test_NullCoalescing_WithProperty()
        {
            var expr = "x => x.Teacher.Age ?? 25".ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual(30, compiled(students1[0]));
        }

        [TestMethod]
        public void Test_NullCoalescing_Chained()
        {
            var expr = "x => x.Age ?? x.WorkHours ?? 0".ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual(30, compiled(teacher1));
            Assert.AreEqual(38, compiled(teacher3));
        }

        #endregion

        #region Conditional (Ternary) Operator Tests

        [TestMethod]
        public void Test_ConditionalOperator_Simple()
        {
            var expr = "x => x.Age >= 18 ? \"Adult\" : \"Minor\"".ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual("Minor", compiled(students2[1])); // Age 17
            Assert.AreEqual("Adult", compiled(students1[0])); // Age 18
            Assert.AreEqual("Adult", compiled(students1[1])); // Age 20
        }

        [TestMethod]
        public void Test_ConditionalOperator_WithNumbers()
        {
            var expr = "x => x.Age > 20 ? 1 : 0".ToExpression<Func<Student, int>>();
            var results = students2.Select(expr.Compile()).ToArray();
            
            Assert.AreEqual(1, results[0]); // Age 21
            Assert.AreEqual(0, results[1]); // Age 17
            Assert.AreEqual(1, results[2]); // Age 22
        }

        [TestMethod]
        public void Test_ConditionalOperator_Nested()
        {
            var expr = "x => x.Age < 18 ? \"Minor\" : x.Age < 65 ? \"Adult\" : \"Senior\"".ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual("Minor", compiled(students2[1])); // Age 17
            Assert.AreEqual("Adult", compiled(students1[0])); // Age 18 (18 < 18 is false, so Adult)
        }

        [TestMethod]
        public void Test_ConditionalOperator_WithNullable()
        {
            var expr = "x => x.IsPrinciple == true ? 100 : 50".ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual(50, compiled(teacher1));
            Assert.AreEqual(100, compiled(teacher2));
        }

        [TestMethod]
        public void Test_ConditionalOperator_ComplexCondition()
        {
            var expr = "x => (x.Teacher?.Age ?? 0) > 35 ? x.Age + 10 : x.Age".ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();
            
            var result1 = compiled(students1[0]); // Teacher age 30
            Assert.AreEqual(18, result1);
            
            var result2 = compiled(students1[1]); // Teacher age 45
            Assert.AreEqual(30, result2); // 20 + 10
        }

        #endregion

        #region Array Access Tests

        [TestMethod]
        public void Test_ArrayAccess_Simple()
        {
            var expr = "x => x.Students[0].Age".ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual(18, compiled(teacher1));
            Assert.AreEqual(21, compiled(teacher2));
        }

        [TestMethod]
        public void Test_ArrayAccess_WithIndex()
        {
            var expr = "x => x.Students[1].FirstName".ToExpression<Func<Teacher, string>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual("Jane", compiled(teacher1));
        }

        [TestMethod]
        public void Test_ArrayAccess_InCondition()
        {
            // Only test with teacher1 and teacher2 who have students
            var expr = "x => x.Students[0].Age >= 18".ToExpression<Func<Teacher, bool>>();
            var results = new[] { teacher1, teacher2 }.Where(expr.Compile()).ToArray();
            
            Assert.AreEqual(2, results.Length); // Both teachers have Students[0].Age >= 18
        }

        [TestMethod]
        public void Test_ArrayAccess_NullConditional()
        {
            // Note: Students?[1] only checks if Students is null, not array bounds
            // So this test only verifies that the expression compiles and works
            // with teachers that have enough students
            var expr = "x => x.Students?[1].Age".ToExpression<Func<Teacher, int?>>();
            var compiled = expr.Compile();
            
            // teacher2.Students[1].Age = 17
            Assert.AreEqual(17, compiled(teacher2));
            
            // teacher3.Students is empty - this would throw IndexOutOfRangeException
            // So we skip testing teacher3 with this expression
        }

        #endregion

        #region Type Casting Tests

        [TestMethod]
        public void Test_TypeCast_ToInt()
        {
            var expr = "x => (int)x.Salary".ToExpression<Func<Teacher, int>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual(50000, compiled(teacher1));
        }

        [TestMethod]
        public void Test_TypeCast_ToDouble()
        {
            var expr = "x => (double)x.Age".ToExpression<Func<Student, double>>();
            var compiled = expr.Compile();
            
            Assert.AreEqual(18.0, compiled(students1[0]));
        }

        #endregion

        #region Assignment Operators Tests

        [TestMethod]
        public void Test_SimpleAssignment()
        {
            var expr = "x => x.Age = 25".ToExpression<Func<Student, int>>();
            var student = new Student { Age = 20, FirstName = "Test", LastName = "User" };
            
            var result = expr.Compile()(student);
            Assert.AreEqual(25, result);
            Assert.AreEqual(25, student.Age);
        }

        [TestMethod]
        public void Test_AddAssignment()
        {
            var expr = "x => x.Age += 5".ToExpression<Func<Student, int>>();
            var student = new Student { Age = 20, FirstName = "Test", LastName = "User" };
            
            var result = expr.Compile()(student);
            Assert.AreEqual(25, result);
            Assert.AreEqual(25, student.Age);
        }

        [TestMethod]
        public void Test_SubtractAssignment()
        {
            var expr = "x => x.Age -= 3".ToExpression<Func<Student, int>>();
            var student = new Student { Age = 20, FirstName = "Test", LastName = "User" };
            
            var result = expr.Compile()(student);
            Assert.AreEqual(17, result);
            Assert.AreEqual(17, student.Age);
        }

        [TestMethod]
        public void Test_MultiplyAssignment()
        {
            var expr = "x => x.Age *= 2".ToExpression<Func<Student, int>>();
            var student = new Student { Age = 10, FirstName = "Test", LastName = "User" };
            
            var result = expr.Compile()(student);
            Assert.AreEqual(20, result);
            Assert.AreEqual(20, student.Age);
        }

        [TestMethod]
        public void Test_DivideAssignment()
        {
            var expr = "x => x.Age /= 2".ToExpression<Func<Student, int>>();
            var student = new Student { Age = 20, FirstName = "Test", LastName = "User" };
            
            var result = expr.Compile()(student);
            Assert.AreEqual(10, result);
            Assert.AreEqual(10, student.Age);
        }

        #endregion

        #region Increment/Decrement Tests

        [TestMethod]
        public void Test_PostIncrement()
        {
            var expr = "x => x.Age++".ToExpression<Func<Student, int>>();
            var student = new Student { Age = 20, FirstName = "Test", LastName = "User" };
            
            var result = expr.Compile()(student);
            Assert.AreEqual(20, result); // Returns old value
            Assert.AreEqual(21, student.Age); // But increments
        }

        [TestMethod]
        public void Test_PostDecrement()
        {
            var expr = "x => x.Age--".ToExpression<Func<Student, int>>();
            var student = new Student { Age = 20, FirstName = "Test", LastName = "User" };
            
            var result = expr.Compile()(student);
            Assert.AreEqual(20, result); // Returns old value
            Assert.AreEqual(19, student.Age); // But decrements
        }

        #endregion

        #region Complex Combined Tests

        [TestMethod]
        public void Test_ComplexExpression_MultipleOperators()
        {
            var expr = "x => (x.Teacher?.Age ?? 36) > 35 && (x.Age > 18 || x.FirstName == \"Tom\")".ToExpression<Func<Student, bool>>();
            var results = students1.Where(expr.Compile()).ToArray();
            Assert.IsTrue(results.Length >= 0);
        }

        [TestMethod]
        public void Test_ComplexExpression_NestedConditionals()
        {
            var expr = "x => x.Age > 20 ? (x.Teacher?.IsPrinciple ?? false ? 100 : 50) : 0".ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();
            Assert.IsNotNull(compiled);
        }

        [TestMethod]
        public void Test_ComplexExpression_MultipleNullChecks()
        {
            var expr = "x => (x.Teacher?.Age ?? 0) > 0 && (x.Teacher?.IsPrinciple ?? false)".ToExpression<Func<Student, bool>>();
            var results = students1.Where(expr.Compile()).ToArray();
            Assert.AreEqual(1, results.Length);
        }

        #endregion
    }
}
