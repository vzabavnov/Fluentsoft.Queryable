# Queryable.OuterJoin Method

## Definition
Namespace: FSC.System.Linq  
Assembly: fsc.system.linq.queriable.dll  
Package: [Queryable.OuterJoin](https://www.nuget.org/packages/Queryable.OuterJoin)

Correlates all records from the left table, and the matching records from the right table based on matching keys 

```c#
public static IQueryable<TResult> OuterJoin<TOuter, TInner, TKey, TResult>(this 
    IQueryable<TOuter> outer,
    IQueryable<TInner> inner,
    Expression<Func<TOuter, TKey>> outerKey,
    Expression<Func<TInner, TKey>> innerKey,
    Expression<Func<TOuter, TInner?, TResult>> resultSelector);
```

### Type Parameters
`TOuter`
The type of the elements of the first sequence.

`TInner`
The type of the elements of the second sequence.

`TKey`
The type of the keys returned by the key selector functions.

`TResult`
The type of the result elements.

### Parameters
`outer` [IQueryable](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1?view=net-7.0)\<TOuter>  
The first sequence to join.

`inner` [IEnumerable](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1?view=net-7.0)\<TInner>  
The sequence to join to the first sequence.

`outerKeySelector` [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-7.0)\<Func<TOuter,TKey>>  
A function to extract the join key from each element of the first sequence.

`innerKeySelector` [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-7.0)\<Func<TInner,TKey>>  
A function to extract the join key from each element of the second sequence.

`resultSelector` [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-7.0)\<Func<TOuter,TInner,TResult>>  
A function to create a result element from two matching elements.

## Example
```c#
ctx.Departments.OuterJoin(ctx.Employees,
    z => z.ID,
    z => z.DepartmentID,
    (department, employee) => new
    {
        Department = department.Name,
        Employee = employee.Name,
    })
```

## Initial challenge 

It all started as a problem a lot of people are facing related to the implementation of ***Left Outer Join*** 
in **Entity Framework**. Here is a little reminder of the problem.

Everybody knows Entity Framework (EF for simplicity) supports joins between two collections based on a key 
and equality:
``` C#
[EntityTypeConfiguration(typeof(DepartmentConfig))]
public class Department
{
    public int ID { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

[EntityTypeConfiguration(typeof(EmployeeConfig))]
public class Employee
{
    public int ID { get; set; }

    public string Name { get; set; } = null!;

    public int? DepartmentID { get; set; }

    public Department? Department { get; set;}
}

public class DepartmentEmployee
{
    public Department Department { get; set; }
    public Employee Employee { get; set; }  
}

public class TestDbContext : DbContext
{
    public DbSet<Department> Departments { get; set; } = null!;

    public DbSet<Employee> Employees { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }
}
```
The Inner Join request looks like:
```c#
await using var ctx = _fixture.CreateDbContext(init: true);

var query = ctx.Departments.Join(ctx.Employees,
    z => z.ID,
    z => z.DepartmentID,
        (department, employee) => new
        {
            Department = department.Name,
            Employee = employee.Name,
        });

_output.WriteLine(query.ToQueryString());
```
The SQL output:
```SQL
SELECT d.Name AS Department, e.Name AS Employee
FROM Departments AS d
INNER JOIN Employees AS e ON d.ID = e.department_id
```
As a result, we will have a collection of **Departments**  with matching **Employees**. If an **Departments** 
does not have an associated **Employee** – it will be left out of result.

Time to talk about ***Left Outer Join***. By “the book” the ***Left Outer Join*** is a combination of using 
the **GroupJoin()** method along with **SelectMany()** and **DefaultIfEmpty()** methods. 
For above context it will look like:
```c#
 var query = ctx.Departments
    .GroupJoin(ctx.Employees,
        d => d.ID,
        e => e.DepartmentID,
        (dep, emps) => new { dep, emps })
    .SelectMany(
        g=> g.emps.DefaultIfEmpty(),
        (a, e) => new 
        {
            Department = a.dep.Name,
            Employee = e.Name
        });

_output.WriteLine(query.ToQueryString());
```
The Sql output:
```SQL
SELECT d.Name AS Department, e.Name AS Employee
FROM Departments AS d
LEFT JOIN Employees AS e ON d.ID = e.department_id
```
The EF provider is able to recognize the construct and use a ***LEFT OUTER JOIN*** in the database query. 
We have what we need. Really?

Let’s analyze the last expression.

- `GroupJoin()` correlates the elements of **Department** with Employee by **DepartmentID**.
Its result is a collection of contain anonymous objects with a **Department** and all associated **Employee** 
items.

- `DefaultIfEmpty()` – returns the default item, in our case **null**, if the collection is empty or all 
items in a collection. In our case if no **Employee** are associated with a **Department**, 
then it's employee’s collection will contain one item with **null** value.

- `SelectMany()` – for each department it calls result selector for each associated **Employee**. 
Because Employee’s collection uses **DefaultIfEmpty()** – it will call at least once.

Now we have an expression that will generate ***Left Outer Join*** in database query. 
Is that expression good and usable? No! Let me explain what I mean.

Recall the expression with a simple join from above. There is an easy way to create an **Expression** to get result. 
For instance, this one:
```c#
public static Expression<Func<Department, Employee, DepartmentEmployee>> Expression = 
    (department, employee) => new DepartmentEmployee
    {
        Department = department,
        Employee = employee
    };
```
Using this expression I can write the Join:
```c#
result = ctx.Departments.Join(
    ctx.Employees,
    d => d.ID,
    e => e.DepartmentID,
    Expression);
```
This is much better.It would be nice to have the ability to use a similar expression with that expression 
above for a ***Left Outer Join***. Unfortunately – this is not possible.

Well, “**Not Possible**” is not in my vocabulary. Let’s make it possible! 

We need an extension method which looks like this one:
```c#
public static IQueryable<TResult> OuterJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer,
    IQueryable<TInner> inner,
    Expression<Func<TOuter, TKey>> outerKey,
    Expression<Func<TInner, TKey>> innerKey,
    Expression<Func<TOuter, TInner?, TResult>> resultSelector)
{
    ...
}
```

It has very similar signature as the 
[Join()](https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.join?view=net-7.0) method. 
Let’s try to make a body of this method using expression for ***Left Outer Join***:
```c#
var query = uter.GroupJoin(inner, outerKey, innerKey, (o, i) => new { o, i }
    .SelectMany(g => g.i.DefaultIfEmpty(), (tmp, i) => new { Outer = tmp.o, Inner = i });
```

The type of **query** is `IQueryable<{TOuter, TInner }>`. It has the fields we need, but it is an anonymous 
type and it is only known only inside the scope of **q2**. Inside a scope we can access to its items but 
we cannot use it to call a result selector expression from outside.

To make it possible to call an external expression we will need some kind of *expression transformer*:
```c#
public static Expression<Func<T, TResult>> ExpressionTransformer<T, TOuter, TInner, TResult>(
    Expression<Func<TOuter, TInner?, TResult>> resultSelector)
{
    …
}
```
which transforms the expression with the anonymous type from **q2** to use **resultSelector** expression. 
It is not so difficult to make by providing a transformation to split the anonymous type’s fields into 
separate instances and here the `SplitParameter` comes to the scene:
```c#
public static class SplitParameter<T1, T2>
{
    public static Expression<Func<T, TResult>> Translate<T, TResult>(
        Expression<Func<T1, T2, TResult>> expression) 
    { 	… 	}

    public static IQueryable<TResult> Select<T, TResult>(
        IQueryable<T> source, 
        Expression<Func<T1, T2, TResult>> selector)
    {
        return source.Select(Translate<T, TResult>(selector));
    }
}
```
Using this method it's easy to implement a method extension for ***Left Outer Join***:
```c#
public static IQueryable<TResult> OuterJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer,
        IQueryable<TInner> inner,
        Expression<Func<TOuter, TKey>> outerKey,
        Expression<Func<TInner, TKey>> innerKey,
        Expression<Func<TOuter, TInner?, TResult>> resultSelector)
    {
        return SplitParameter<TOuter, TInner?>.Select(
            outer
                .GroupJoin(
                    inner,
                    outerKey,
                    innerKey,
                    (o, i) => new { o, i })
                .SelectMany(z => z.i.DefaultIfEmpty(),
                    (t, i) => new { t.o, i }),
            resultSelector);
    }
```
The last piece of the puzzle is using the **ExpressionVisitor** to transform expressions:
```c#
public static Expression<Func<T, TResult>> Translate<T, TResult>(
    Expression<Func<T1, T2, TResult>> expression)
{
    return new SplitParameterVisitor<T, TResult>(expression).Translate();
}

private class SplitParameterVisitor<T, TResult> : ExpressionVisitor
{
    private readonly MemberExpression _arg1Expression;
    private readonly MemberExpression _arg2Expression;
    private readonly Expression<Func<T1, T2, TResult>> _expression;
    private readonly ParameterExpression _p1;
    private readonly ParameterExpression _p2;
    private readonly ParameterExpression _parameterExpression;

    public SplitParameterVisitor(Expression<Func<T1, T2, TResult>> expression)
    {
        _expression = expression;

        _p1 = expression.Parameters[0];
        _p2 = expression.Parameters[1];

        var type = typeof(T);

        const BindingFlags FLAGS = BindingFlags.Instance 
            | BindingFlags.Public 
            | BindingFlags.GetProperty;

        var properties = type.GetProperties(FLAGS).ToArray();

        var prop1Name = properties
            .Where(z => z.PropertyType.IsAssignableTo(typeof(T1)))
            .Select(z => z.Name)
            .FirstOrDefault();

        var prop2Name = typeof(T1) == typeof(T2)
            ? properties.Where(z => z.PropertyType.IsAssignableTo(typeof(T2)))
                .Skip(1)
                .Select(z => z.Name).FirstOrDefault()
            : properties.Where(z => z.PropertyType.IsAssignableTo(typeof(T2)))
                .Select(z => z.Name).FirstOrDefault();

        _parameterExpression = Expression.Parameter(type);
        
        _arg1Expression = Expression.Property(_parameterExpression, prop1Name);
        
        _arg2Expression = Expression.Property(_parameterExpression, prop2Name);
    }

    public Expression<Func<T, TResult>> Translate()
    {
        return Expression.Lambda<Func<T, TResult>>(base.Visit(_expression.Body), _parameterExpression);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (node == _p1)
        {
            return _arg1Expression;
        }

        if (node == _p2)
        {
            return _arg2Expression;
        }

        return base.VisitParameter(node);	
    }
}
```
Here in the visitor’s constructor, we are looking for properties of type **T1** and **T2** and making 
expression **Property**:
```c#
_parameterExpression = Expression.Parameter(type);
_arg1Expression = Expression.Property(_parameterExpression, prop1Name);
_arg2Expression = Expression.Property(_parameterExpression, prop2Name);
```
In addition, expression parameter created based on type T as well as getting parameters from argument 
expression: 
```c#
_p1 = expression.Parameters[0];
_p2 = expression.Parameters[1];
```
At last, in the **VisitParameter** override method, we replace all occurrence of **_p1** and **_p2** nodes 
by **arg1Expression** and **arg2Expression** respectively. 

The last call is to the **Translate** method, which just executes an expression visitor.

Finally, we can write the ***Left Outer Join*** expression:
```c#
query = ctx.Departments.OuterJoin(ctx.Employees,
    z => z.ID,
    z => z.DepartmentID,
    (department, employee) => new
    {
        Department = department.Name,
        Employee = employee.Name,
    });

    _output.WriteLine(query.ToQueryString());
);
```
The Sql output:
```SQL
SELECT d.Name AS Department, e.Name AS Employee
FROM Departments AS d
LEFT JOIN Employees AS e ON d.ID = e.department_id
```
and this is excatly the same SQL request from above.

## Puzzle Soved!