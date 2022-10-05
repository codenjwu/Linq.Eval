# Linq.Eval
## Write Linq with Linq.Eval just like write native c# code and python eval

**example**

```csharp
var expr1 = "(x,y)=>x.Teacher.Age ".ToExpression<Func<Student, Teacher, int?>>();
var expr2 = "(x,y)=>x.Teacher.IsPrinciple? 0:100 ".ToExpression<Func<Student, Teacher, int>>();
var expr3 = "(x,y)=>x.Teacher.Age?? 0".ToExpression<Func<Student, Teacher, int>>();
var expr4 = "(x,y)=>x.Teacher?.Age ".ToExpression<Func<Student, Teacher, int?>>();
var expr5 = "(x,y)=>(x.Teacher?.Age??36) > 35 && (x.Age > 10 || !y.IsPrinciple)".ToExpression<Func<Student, Teacher, bool>>();

```
