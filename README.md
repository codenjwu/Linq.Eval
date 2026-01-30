# Linq.Eval

[![NuGet](https://img.shields.io/nuget/v/Linq.Eval.svg)](https://www.nuget.org/packages/Linq.Eval/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)

## Write Linq with Linq.Eval just like writing native C# code and Python eval

Convert string expressions to LINQ expressions dynamically with full support for nullable types, complex operations, and multi-parameter lambdas.

### ✨ Features

- 🎯 **Dynamic Expression Parsing** - Convert strings to compiled LINQ expressions
- 🔧 **Nullable Type Support** - Full support for `??`, `?.` operators
- 🧮 **Complex Expressions** - Nested arithmetic, logical operations, conditionals
- 👥 **Multi-Parameter Lambdas** - Two-parameter lambda support
- 🔒 **Type Safe** - Proper Expression Tree type checking
- 🚀 **Multi-Framework** - Supports .NET 6.0, 7.0, 8.0, 9.0
- ✅ **Battle Tested** - 970+ passing tests

### 📦 Installation

```bash
dotnet add package Linq.Eval
```

### 🚀 Quick Start

```csharp
using Linq.Eval;

// Simple property access
var expr1 = "(x,y)=>x.Teacher.Age".ToExpression<Func<Student, Teacher, int?>>();

// Conditional expressions
var expr2 = "(x,y)=>x.Teacher.IsPrinciple ? 0 : 100".ToExpression<Func<Student, Teacher, int>>();

// Null-coalescing operator
var expr3 = "(x,y)=>x.Teacher.Age ?? 0".ToExpression<Func<Student, Teacher, int>>();

// Null-conditional operator
var expr4 = "(x,y)=>x.Teacher?.Age".ToExpression<Func<Student, Teacher, int?>>();

// Complex expressions
var expr5 = "(x,y)=>(x.Teacher?.Age ?? 36) > 35 && (x.Age > 10 || !y.IsPrinciple)".ToExpression<Func<Student, Teacher, bool>>();
```

### 📚 Advanced Examples

#### Single Parameter Lambda
```csharp
// Arithmetic with nullable types
var expr = "x => (x.Salary ?? 0.0) / (double)x.WorkHours > 1000.0"
    .ToExpression<Func<Teacher, bool>>();

// String operations
var expr = "x => x.FirstName.Length > 5 && x.LastName.StartsWith(\"S\")"
    .ToExpression<Func<Teacher, bool>>();

// Complex nested conditions
var expr = "x => (x.Age ?? 0) > 30 ? (x.Salary ?? 0.0) * 2.0 : (x.Salary ?? 0.0)"
    .ToExpression<Func<Teacher, double>>();
```

#### Two Parameter Lambda
```csharp
// Compare two objects
var expr = "(x, y) => (x.Age ?? 0) < (y.Age ?? 0) ? x.Salary : y.Salary"
    .ToExpression<Func<Teacher, Teacher, double?>>();

// Mixed type parameters
var expr = "(x, y) => (double)(x.Age ?? 0) > y ? (x.Salary ?? 0.0) : y"
    .ToExpression<Func<Teacher, double, double>>();
```

### 🎯 What's New in v1.1.0

- ✅ **200+ Bug Fixes** - Enhanced nullable type handling and type conversions
- 🧪 **172 New Tests** - Comprehensive test coverage for edge cases
- 🔧 **Improved Type Safety** - Better Expression Tree type matching
- 📈 **99.6% Test Pass Rate** - 968 passing tests across 4 frameworks

See [CHANGELOG.md](CHANGELOG.md) for detailed release notes.

### 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

### 🔗 Links

- [GitHub Repository](https://github.com/codenjwu/Linq.Eval)
- [NuGet Package](https://www.nuget.org/packages/Linq.Eval/)
- [Report Issues](https://github.com/codenjwu/Linq.Eval/issues)
