# Queryable Class
## Definitions 
Namespace: Fluentsoft.System.Linq  
Assembly: fluentsoft.system.linq.queryable.dll  
Package: [Fluentsoft.Queryable](https://www.nuget.org/packages/Fluentsoft.Queryable)

Provides a set of static (Shared in Visual Basic) methods for querying data structures that implement [IQueryable<T>](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1?view=net-7.0).

```c#
public static class Queryable
```

## Methods

| *Methods*      | *Description* |
|--------------|-------------|
|LeftOuterJoin|Correlates all records from the left table, and the matching records from the right table based on matching keys |
|RightOuterJoin|Correlates all records from the right table, and the matching records from the left table based on matching keys |
|FullOuterJoin|Correlates all records from the right table, and all records from the left table based on matching keys |
|Select<T, TResult, T1, T2>(IQueryable<T> source, Expression<Func<T1, T2, TResult>> selector)|Projects each element of a sequence into a new form.|
|IQueryable<TResult> Select<T, TResult, T1, T2, T3>(IQueryable<T> source, Expression<Func<T1, T2, T3, TResult>> selector)|Projects each element of a sequence into a new form.|
|IQueryable<T> Where<T, T1, T2, T3>(this IQueryable<T> source, Expression<Func<T1, T2, T3, bool>> predicate)|Filters a sequence of values based on a predicate with three arguments taken from source value by splitting.|
|IQueryable<T> Where<T, T1, T2>(IQueryable<T> source, Expression<Func<T1, T2, bool>> predicate)|Filters a sequence of values based on a predicate with two arguments taken from source value by splitting.|

---

Correlates all records from the left table, and the matching records from the right table based on matching keys 

```c#
public static IQueryable<TResult> LeftOuterJoin<TOuter, TInner, TKey, TResult>(this 
    IQueryable<TOuter> outer,
    IEnumerable<TInner> inner,
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

### Returns
[IQueryable](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1?view=net-7.0)\<TResult>  
An [IQueryable\<T>](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1?view=net-7.0) that has elements of type TResult obtained by performing an left outer join on two sequences.

## Example
```c#
ctx.Departments.LeftOuterJoin(ctx.Employees,
    z => z.ID,
    z => z.DepartmentID,
    (department, employee) => new
    {
        Department = department.Name,
        Employee = employee.Name,
    })
```

---
```c#
public static IQueryable<TResult> RughtOuterJoin<TOuter, TInner, TKey, TResult>(this 
    IEnumerable<TOuter> outer,
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
`outer` [IEnumerable](https://learn.microsoft.com/en-us/dotnet/api/system.linq.ienumerable-1?view=net-7.0)\<TOuter>  
The first sequence to join.

`inner` [IQueryable](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iqueryable-1?view=net-7.0)\<TInner>  
The sequence to join to the first sequence.

`outerKeySelector` [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-7.0)\<Func<TOuter,TKey>>  
A function to extract the join key from each element of the first sequence.

`innerKeySelector` [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-7.0)\<Func<TInner,TKey>>  
A function to extract the join key from each element of the second sequence.

`resultSelector` [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-7.0)\<Func<TOuter,TInner,TResult>>  
A function to create a result element from two matching elements.

### Returns
[IQueryable](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1?view=net-7.0)\<TResult>  
An [IQueryable\<T>](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1?view=net-7.0) that has elements of type TResult obtained by performing an right outer join on two sequences.

## Example
```c#
ctx.Departments.RightOuterJoin(ctx.Employees,
    z => z.ID,
    z => z.DepartmentID,
    (department, employee) => new
    {
        Department = department.Name,
        Employee = employee.Name,
    })
```

---

```c#
public static IQueryable<TResult> FullOuterJoin<TOuter, TInner, TKey, TResult>(this 
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

`inner` [IQueryable](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iqueryable-1?view=net-7.0)\<TInner>  
The sequence to join to the first sequence.

`outerKeySelector` [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-7.0)\<Func<TOuter,TKey>>  
A function to extract the join key from each element of the first sequence.

`innerKeySelector` [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-7.0)\<Func<TInner,TKey>>  
A function to extract the join key from each element of the second sequence.

`resultSelector` [Expression](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-7.0)\<Func<TOuter,TInner,TResult>>  
A function to create a result element from two matching elements.

### Returns
[IQueryable](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1?view=net-7.0)\<TResult>  
An [IQueryable\<T>](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1?view=net-7.0) that has elements of type TResult 
obtained by performing an full outer join on two sequences.

## Example
```c#
ctx.Departments.FullOuterJoin(ctx.Employees,
    z => z.ID,
    z => z.DepartmentID,
    (department, employee) => new
    {
        Department = department.Name,
        Employee = employee.Name,
    })
```
---

## Publication: [Implementation of Left Outer Join for Entity Framework](https://medium.com/@zabavnov/implementation-of-left-outer-join-for-entity-framework-b47469633e2f)