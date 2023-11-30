// Copyright 2023 Fluent Software Corporation
// Author: Vadim Zabavnov (mailto:vzabavnov@fluentsoft.net; mailto:zabavnov@gmail.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;

namespace Fluentsoft.System.Linq.Queryable.Tests;

public class ExpressionsTests : IClassFixture<ContextFixture>
{
    private readonly ContextFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ExpressionsTests(ContextFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task CheckConnection()
    {
        await using var ctx = _fixture.CreateDbContext();
#if NET6_0
        await ctx.Database.ExecuteSqlRawAsync($"Select 1");
#else
        await ctx.Database.ExecuteSqlAsync($"Select 1");
#endif
    }

    [Fact]
    public async Task CheckInnerJoin()
    {
        await using var ctx = _fixture.CreateDbContext();

        var query = ctx.Departments.Join(ctx.Employees,
            z => z.ID,
            z => z.DepartmentID,
            (department, employee) => new
            {
                Department = department.Name,
                Employee = employee.Name
            });

        _output.WriteLine(query.ToQueryString());

        var result = await query.ToArrayAsync();
        Assert.True(result.Length > 0);
    }

    [Fact]
    public void TestFullOuterJoin()
    {
        using var ctx = _fixture.CreateDbContext();

        var query = ctx.Departments.FullOuterJoin(ctx.Employees,
            z => z.ID,
            z => z.DepartmentID,
            (d, e) => new
            {
                Department = d != null ? d.Name : null,
                Employee = e != null ? e.Name : null
            });

        var str1 = query.ToQueryString();

        _output.WriteLine(str1);
    }

    [Fact]
    public void TestRightOuterJoin()
    {
        using var ctx = _fixture.CreateDbContext();

        var query = ctx.Departments.RightOuterJoin(ctx.Employees,
            z => z.ID,
            z => z.DepartmentID,
            (d, employee) => new
            {
                Department = d != null ? d.Name : null,
                Employee = employee.Name
            });

        var str1 = query.ToQueryString();

        _output.WriteLine(str1);
    }

    [Fact]
    public async Task TestLeftOuterJoin()
    {
        await using var ctx = _fixture.CreateDbContext();

        var query = ctx.Departments.GroupJoin(ctx.Employees,
                d => d.ID,
                e => e.DepartmentID,
                (department, employees) => new
                {
                    department,
                    employees
                })
            .SelectMany(g => g.employees.DefaultIfEmpty(),
                (a, e) => new
                {
                    Department = a.department.Name,
                    Employee = e != null ? e.Name : null
                });

        var str1 = query.ToQueryString();

        _output.WriteLine(str1);

        query = ctx.Departments.LeftOuterJoin(ctx.Employees,
            z => z.ID,
            z => z.DepartmentID,
            (department, employee) => new
            {
                Department = department.Name,
                Employee = employee != null ? employee.Name : null
            });

        var str2 = query.ToQueryString();
        _output.WriteLine(str2);

        Assert.Equal(str1, str2);

        var result = await query.ToArrayAsync();
        Assert.True(result.Length > 0);
    }

    [Fact]
    public void TestSwitch()
    {
        Expression<Func<int, string, KeyValuePair<int, string>>> sourceExpr = (i, s) => new KeyValuePair<int, string>(i, s);

        var targetExpr = sourceExpr.SwitchParameters();

        var f1 = sourceExpr.Compile();
        var f2 = targetExpr.Compile();

        Assert.Equal(f1(123, "123"), f2("123", 123));
    }

    [Fact]
    public void TestSplitExpression()
    {
        Expression<Func<int, string, string>> sourceExpr = (i, s) => s + i;

        var targetExpr = sourceExpr.SplitParameters<int, string, KeyValuePair<int, string>, string>();
        var f1 = sourceExpr.Compile();
        var f2 = targetExpr.Compile();
        var s1 = f1(2, "5");
        Assert.Equal("52", s1);

        var s2 = f2(new KeyValuePair<int, string>(2, "5"));
        Assert.Equal("52", s2);
    }

    [Fact]
    public void TestSplitExpressionTwoSameType()
    {
        Expression<Func<int, int, string>> sourceExpr = (i1, i2) => i2 + i1.ToString();

        var targetExpr = sourceExpr.SplitParameters<int, int, KeyValuePair<int, int>, string>();
        var f1 = sourceExpr.Compile();
        var f2 = targetExpr.Compile();
        var s1 = f1(2, 5);
        Assert.Equal("52", s1);

        var s2 = f2(new KeyValuePair<int, int>(2, 5));
        Assert.Equal("52", s2);
    }

    [Fact]
    public void TestSplitExpressionThreeArguments()
    {
        Expression<Func<int, string, bool, string>> sourceExpr = (i, s, b) => s + i + b;

        var targetExpr = sourceExpr.SplitParameters<int, string, bool, Tuple<int, string, bool>, string>();
        var f1 = sourceExpr.Compile();
        var f2 = targetExpr.Compile();
        var s1 = f1(2, "5", true);
        Assert.Equal("52True", s1);

        var s2 = f2(new Tuple<int, string, bool>(2, "5", true));
        Assert.Equal("52True", s2);
    }

    [Fact]
    public async Task TestSelectSplit()
    {
        await using var ctx = _fixture.CreateDbContext();

        var query = ctx.Employees
            .Select(z => new
        {
                z.Name,
                z.Department,
                z.DepartmentID,
        })
            .Select((string name, Department department, int? departmentID) => name + department.Name + (departmentID.HasValue ? departmentID.Value.ToString() : ""));
        
        var s = query.ToQueryString();
    }
}