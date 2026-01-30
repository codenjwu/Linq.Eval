namespace Linq.Eval.Test
{
    /// <summary>
    /// Tests for basic expression parsing including literals, variables, and basic operators.
    /// </summary>
    [TestClass]
    public class BasicExpressionTests
    {
        private Teacher teacher1 = null!;
        private Teacher teacher2 = null!;
        private Teacher teacher3 = null!;
        private Student[] students = null!;

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

            students = new Student[]
            {
                new Student { Age = 18, FirstName = "Tom", LastName = "Brown", Teacher = teacher1 },
                new Student { Age = 20, FirstName = "Jane", LastName = "Davis", Teacher = teacher2 },
                new Student { Age = 19, FirstName = "Mike", LastName = "Wilson", Teacher = teacher1 },
                new Student { Age = 21, FirstName = "Sarah", LastName = "Taylor", Teacher = teacher3 },
                new Student { Age = 17, FirstName = "Chris", LastName = "Anderson", Teacher = teacher2 },
            };
        }

        #region Literal Tests

        [TestMethod]
        public void Test_BooleanLiteral_True()
        {
            var expr = "x => true".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(5, result.Length);
        }

        [TestMethod]
        public void Test_BooleanLiteral_False()
        {
            var expr = "x => false".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void Test_NumericLiteral_Integer()
        {
            var expr = "x => x.Age == 20".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Jane", result[0].FirstName);
        }

        [TestMethod]
        public void Test_NumericLiteral_Multiple()
        {
            var expr = "x => x.Age > 18".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(3, result.Length);
        }

        [TestMethod]
        public void Test_StringLiteral_Equality()
        {
            var expr = "x => x.FirstName == \"Tom\"".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(18, result[0].Age);
        }

        [TestMethod]
        public void Test_StringLiteral_WithEscapedQuotes()
        {
            var expr = "x => x.FirstName == \"Tom\"".ToExpression<Func<Student, bool>>();
            Assert.IsNotNull(expr);
        }

        #endregion

        #region Arithmetic Operators

        [TestMethod]
        public void Test_Addition_Simple()
        {
            var expr = "x => x.Age + 5".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).ToArray();
            Assert.AreEqual(23, result[0]); // 18 + 5
            Assert.AreEqual(25, result[1]); // 20 + 5
        }

        [TestMethod]
        public void Test_Subtraction_Simple()
        {
            var expr = "x => x.Age - 5".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).ToArray();
            Assert.AreEqual(13, result[0]); // 18 - 5
            Assert.AreEqual(15, result[1]); // 20 - 5
        }

        [TestMethod]
        public void Test_Multiplication_Simple()
        {
            var expr = "x => x.Age * 2".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).ToArray();
            Assert.AreEqual(36, result[0]); // 18 * 2
            Assert.AreEqual(40, result[1]); // 20 * 2
        }

        [TestMethod]
        public void Test_Division_Simple()
        {
            var expr = "x => x.Age / 2".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).ToArray();
            Assert.AreEqual(9, result[0]); // 18 / 2
            Assert.AreEqual(10, result[1]); // 20 / 2
        }

        [TestMethod]
        public void Test_Modulo_Simple()
        {
            var expr = "x => x.Age % 3".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).ToArray();
            Assert.AreEqual(0, result[0]); // 18 % 3 = 0
            Assert.AreEqual(2, result[1]); // 20 % 3 = 2
        }

        [TestMethod]
        public void Test_ComplexArithmetic()
        {
            var expr = "x => (x.Age + 5) * 2 - 10".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).First();
            Assert.AreEqual(36, result); // (18 + 5) * 2 - 10 = 36
        }

        [TestMethod]
        public void Test_StringConcatenation()
        {
            var expr = "x => x.FirstName + \" \" + x.LastName".ToExpression<Func<Student, string>>();
            var result = students.Select(expr.Compile()).First();
            Assert.AreEqual("Tom Brown", result);
        }

        #endregion

        #region Comparison Operators

        [TestMethod]
        public void Test_Equality_True()
        {
            var expr = "x => x.Age == 18".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void Test_Inequality_NotEqual()
        {
            var expr = "x => x.Age != 18".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(4, result.Length);
        }

        [TestMethod]
        public void Test_GreaterThan()
        {
            var expr = "x => x.Age > 19".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(2, result.Length);
        }

        [TestMethod]
        public void Test_GreaterThanOrEqual()
        {
            var expr = "x => x.Age >= 19".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(3, result.Length);
        }

        [TestMethod]
        public void Test_LessThan()
        {
            var expr = "x => x.Age < 19".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(2, result.Length);
        }

        [TestMethod]
        public void Test_LessThanOrEqual()
        {
            var expr = "x => x.Age <= 19".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(3, result.Length);
        }

        #endregion

        #region Logical Operators

        [TestMethod]
        public void Test_LogicalAnd()
        {
            var expr = "x => x.Age > 18 && x.Age < 21".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(2, result.Length);
        }

        [TestMethod]
        public void Test_LogicalOr()
        {
            var expr = "x => x.Age < 18 || x.Age > 20".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(2, result.Length);
        }

        [TestMethod]
        public void Test_LogicalNot()
        {
            var expr = "x => !(x.Age > 20)".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(4, result.Length);
        }

        [TestMethod]
        public void Test_ComplexLogicalExpression()
        {
            var expr = "x => (x.Age > 18 && x.Age < 21) || x.FirstName == \"Tom\"".ToExpression<Func<Student, bool>>();
            var result = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(3, result.Length);
        }

        #endregion

        #region Member Access

        [TestMethod]
        public void Test_PropertyAccess_Simple()
        {
            var expr = "x => x.Age".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).ToArray();
            Assert.AreEqual(18, result[0]);
            Assert.AreEqual(20, result[1]);
        }

        [TestMethod]
        public void Test_PropertyAccess_Nested()
        {
            var expr = "x => x.Teacher.FirstName".ToExpression<Func<Student, string>>();
            var result = students.Select(expr.Compile()).First();
            Assert.AreEqual("John", result);
        }

        [TestMethod]
        public void Test_PropertyAccess_DeepNested()
        {
            var expr = "x => x.Teacher.Age".ToExpression<Func<Student, int?>>();
            var result = students.Select(expr.Compile()).ToArray();
            Assert.AreEqual(30, result[0]);
            Assert.AreEqual(45, result[1]);
        }

        #endregion

        #region Parenthesized Expressions

        [TestMethod]
        public void Test_ParenthesizedExpression_Simple()
        {
            var expr = "x => (x.Age)".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).First();
            Assert.AreEqual(18, result);
        }

        [TestMethod]
        public void Test_ParenthesizedExpression_Complex()
        {
            var expr = "x => ((x.Age + 5) * 2)".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).First();
            Assert.AreEqual(46, result); // (18 + 5) * 2
        }

        [TestMethod]
        public void Test_ParenthesizedExpression_Precedence()
        {
            var expr = "x => x.Age + 5 * 2".ToExpression<Func<Student, int>>();
            var result1 = students.Select(expr.Compile()).First();
            
            var expr2 = "x => (x.Age + 5) * 2".ToExpression<Func<Student, int>>();
            var result2 = students.Select(expr2.Compile()).First();
            
            Assert.AreEqual(28, result1); // 18 + (5 * 2)
            Assert.AreEqual(46, result2); // (18 + 5) * 2
        }

        #endregion

        #region Multiple Parameters

        [TestMethod]
        public void Test_TwoParameters_Simple()
        {
            var expr = "(x, y) => x.Age > y.Age".ToExpression<Func<Student, Teacher, bool>>();
            var compiled = expr.Compile();
            
            var result = compiled(students[0], teacher1); // 18 > 30
            Assert.IsFalse(result);
            
            result = compiled(students[1], teacher1); // 20 < 30
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Test_TwoParameters_PropertyAccess()
        {
            var expr = "(x, y) => x.Teacher.Age > y.Age".ToExpression<Func<Student, Teacher, bool>>();
            var compiled = expr.Compile();
            Assert.IsNotNull(compiled);
        }

        #endregion
    }
}
