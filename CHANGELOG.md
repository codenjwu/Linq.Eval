# Changelog

All notable changes to this project will be documented in this file.

## [1.1.0] - 2026-01-29

### Added
- **172 new comprehensive tests** across 4 new test files:
  - ComplexExpressionTests.cs (52 tests) - Complex arithmetic, logical operations, nested expressions
  - NullHandlingTests.cs (35 tests) - Nullable type handling, null-coalescing, null-conditional operators
  - StringOperationTests.cs (29 tests) - String operations mixed with numeric/boolean logic
  - TwoParameterLambdaTests.cs (56 tests) - Two-parameter lambda expressions with various types
- Enhanced nullable type support with proper type conversions
- Improved error handling for Expression Tree type mismatches

### Fixed
- **200+ edge cases** related to nullable types and type conversions
- Binary operator type mismatches between `double?` and `int`
- Null coalescing operator behavior with mixed numeric types
- Return type issues in conditional expressions
- Division operations with nullable types
- Complex nested expressions with proper parenthesis handling

### Improved
- Test coverage increased from ~70 tests to **242 passing tests** (99.6% pass rate)
- Multi-framework testing (net6.0, net7.0, net8.0, net9.0) - **968 total passing tests**
- Better handling of complex arithmetic with nullable properties
- Type safety in Expression Tree compilation

### Technical Details
- All tests now properly handle Expression Tree type system constraints
- Consistent use of double literals (`.0` suffix) for floating-point comparisons
- Proper null coalescing patterns: `(x.Salary ?? 0.0)` instead of `(x.Salary ?? 0)`
- Type conversions: `(double)(x.Age ?? 0)` for mixed-type operations

## [1.0.0] - Initial Release

### Added
- Basic LINQ expression parsing from strings
- Support for lambda expressions
- Nullable type operators (`??`, `?.`)
- Conditional operators
- Basic arithmetic and logical operations
