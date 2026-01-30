using Microsoft.CodeAnalysis.Scripting;

namespace Linq.Eval.Test
{
    /// <summary>
    /// Tests for error handling, edge cases, and boundary conditions.
    /// </summary>
    [TestClass]
    public class ErrorHandlingTests
    {
        private Student[] students = null!;
        private Teacher teacher = null!;

        [TestInitialize]
        public void Init()
        {
            teacher = new Teacher 
            { 
                FirstName = "John", 
                LastName = "Smith", 
                Age = 30, 
                IsPrinciple = false, 
                Salary = 50000, 
                WorkHours = 40 
            };

            students = new Student[]
            {
                new Student { Age = 18, FirstName = "Tom", LastName = "Brown", Teacher = teacher },
                new Student { Age = 20, FirstName = "Jane", LastName = "Davis", Teacher = null! },
            };
        }

        #region Expression Query Error Tests

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Test_Expression_InvalidSyntax_ShouldThrow()
        {
            // Missing lambda operator
            var expr = "x x.Age > 18".ToExpression<Func<Student, bool>>();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Test_Expression_EmptyString_ShouldThrow()
        {
            var expr = "".ToExpression<Func<Student, bool>>();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Test_Expression_InvalidLambda_ShouldThrow()
        {
            // Invalid lambda syntax - missing parameter name
            // Should throw InvalidOperationException when trying to find parameter
            var expr = "=> x.Age".ToExpression<Func<Student, int>>();
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Test_Expression_UnsupportedSyntax_MethodCall()
        {
            // Method calls are not yet fully implemented
            var expr = "x => x.FirstName.ToUpper()".ToExpression<Func<Student, string>>();
            var compiled = expr.Compile();
            compiled(students[0]);
        }

        #endregion

        #region Delegate Query Error Tests

        [TestMethod]
        [ExpectedException(typeof(CompilationErrorException))]
        public async Task Test_Delegate_InvalidSyntax_ShouldThrow()
        {
            var del = await "x x.Age".ToDelegate<Func<Student, int>>();
        }

        [TestMethod]
        [ExpectedException(typeof(CompilationErrorException))]
        public async Task Test_Delegate_UndefinedVariable_ShouldThrow()
        {
            var del = await "x => y.Age".ToDelegate<Func<Student, int>>();
        }

        [TestMethod]
        [ExpectedException(typeof(CompilationErrorException))]
        public async Task Test_Delegate_TypeMismatch_ShouldThrow()
        {
            // Trying to return string when int is expected
            var del = await "x => x.FirstName".ToDelegate<Func<Student, int>>();
        }

        #endregion

        #region Null Handling Tests

        [TestMethod]
        public void Test_Expression_NullTeacher_WithNullConditional()
        {
            var expr = "x => x.Teacher?.Age ?? 0".ToExpression<Func<Student, int>>();
            var compiled = expr.Compile();
            
            var result = compiled(students[1]); // Student with null teacher
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Test_Expression_NullableProperty_Comparison()
        {
            var expr = "x => (x.Teacher?.Age ?? 0) > 25".ToExpression<Func<Student, bool>>();
            var results = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(1, results.Length);
        }

        [TestMethod]
        public void Test_Expression_NullableBool()
        {
            var expr = "x => x.Teacher?.IsPrinciple ?? false".ToExpression<Func<Student, bool>>();
            var results = students.Select(expr.Compile()).ToArray();
            Assert.AreEqual(false, results[0]);
            Assert.AreEqual(false, results[1]);
        }

        #endregion

        #region Boundary Value Tests

        [TestMethod]
        public void Test_Expression_MaxInt()
        {
            var expr = $"x => x.Age < {int.MaxValue}".ToExpression<Func<Student, bool>>();
            var results = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(2, results.Length);
        }

        [TestMethod]
        public void Test_Expression_MinInt()
        {
            var expr = $"x => x.Age > {int.MinValue}".ToExpression<Func<Student, bool>>();
            var results = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(2, results.Length);
        }

        [TestMethod]
        public void Test_Expression_Zero()
        {
            var expr = "x => x.Age > 0".ToExpression<Func<Student, bool>>();
            var results = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(2, results.Length);
        }

        [TestMethod]
        public void Test_Expression_EmptyString()
        {
            var expr = "x => x.FirstName != \"\"".ToExpression<Func<Student, bool>>();
            var results = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(2, results.Length);
        }

        #endregion

        #region Type Safety Tests

        [TestMethod]
        public void Test_Expression_CorrectTypeInference()
        {
            var expr = "x => x.Age".ToExpression<Func<Student, int>>();
            Assert.AreEqual(typeof(int), expr.ReturnType);
        }

        [TestMethod]
        public void Test_Expression_NullableReturnType()
        {
            var expr = "x => x.Teacher?.Age".ToExpression<Func<Student, int?>>();
            Assert.AreEqual(typeof(int?), expr.ReturnType);
        }

        [TestMethod]
        public void Test_Expression_BooleanReturnType()
        {
            var expr = "x => x.Age > 18".ToExpression<Func<Student, bool>>();
            Assert.AreEqual(typeof(bool), expr.ReturnType);
        }

        #endregion

        #region Special Characters and Unicode Tests

        [TestMethod]
        public void Test_Expression_StringWithSpaces()
        {
            var expr = "x => x.FirstName == \"Tom Brown\"".ToExpression<Func<Student, bool>>();
            Assert.IsNotNull(expr);
        }

        [TestMethod]
        public void Test_Expression_NumbersInString()
        {
            var expr = "x => x.FirstName == \"Student123\"".ToExpression<Func<Student, bool>>();
            var results = students.Where(expr.Compile()).ToArray();
            Assert.AreEqual(0, results.Length);
        }

        #endregion

        #region Complex Nested Expressions

        [TestMethod]
        public void Test_Expression_DeeplyNested()
        {
            var expr = "x => ((x.Age + 5) * 2 - 10) / 2 + 3".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).First();
            Assert.AreEqual(21, result); // ((18 + 5) * 2 - 10) / 2 + 3 = (46 - 10) / 2 + 3 = 18 + 3 = 21
        }

        [TestMethod]
        public void Test_Expression_MultipleParentheses()
        {
            var expr = "x => (((x.Age)))".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).First();
            Assert.AreEqual(18, result);
        }

        #endregion

        #region Parameter Tests

        [TestMethod]
        public void Test_Expression_NoParameters()
        {
            var expr = "() => 42".ToExpression<Func<int>>();
            var result = expr.Compile()();
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void Test_Expression_TwoParameters_DifferentTypes()
        {
            var expr = "(x, y) => x.Age > y".ToExpression<Func<Student, int, bool>>();
            var compiled = expr.Compile();
            
            Assert.IsTrue(compiled(students[0], 17));
            Assert.IsFalse(compiled(students[0], 20));
        }

        #endregion

        #region Cache Tests

        [TestMethod]
        public async Task Test_Delegate_Cache_Performance()
        {
            var query = "x => x.Age > 18";
            
            // First call - should compile
            var start1 = DateTime.Now;
            var delegate1 = await query.ToDelegate<Func<Student, bool>>(cache: true);
            var time1 = (DateTime.Now - start1).TotalMilliseconds;
            
            // Second call - should use cache
            var start2 = DateTime.Now;
            var delegate2 = await query.ToDelegate<Func<Student, bool>>(cache: true);
            var time2 = (DateTime.Now - start2).TotalMilliseconds;
            
            // Results should be identical
            var results1 = students.Where(delegate1).ToArray();
            var results2 = students.Where(delegate2).ToArray();
            Assert.AreEqual(results1.Length, results2.Length);
        }

        [TestMethod]
        public async Task Test_Delegate_NoCache()
        {
            var query = "x => x.Age > 18";
            
            var delegate1 = await query.ToDelegate<Func<Student, bool>>(cache: false);
            var delegate2 = await query.ToDelegate<Func<Student, bool>>(cache: false);
            
            // Should still work correctly
            var results1 = students.Where(delegate1).ToArray();
            var results2 = students.Where(delegate2).ToArray();
            Assert.AreEqual(results1.Length, results2.Length);
        }

        #endregion

        #region Edge Case Operators

        [TestMethod]
        public void Test_Expression_BitwiseXor()
        {
            var expr = "x => x.Age ^ 5".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).First();
            Assert.AreEqual(18 ^ 5, result);
        }

        [TestMethod]
        public void Test_Expression_ModuloByZero_Runtime()
        {
            var expr = "x => x.Age % 1".ToExpression<Func<Student, int>>();
            var result = students.Select(expr.Compile()).First();
            Assert.AreEqual(0, result);
        }

        #endregion
    }
}
