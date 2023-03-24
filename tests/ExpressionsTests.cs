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

using FSC.System.Linq;
using System.Linq.Expressions;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FSC.EntityFrameworkCore.Expressions.Tests;

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

        await ctx.Database.ExecuteSqlAsync($"Select 1");
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
                Employee = employee.Name,
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
            (department, employee) => new
            {
                Department = department.Name,
                Employee = employee.Name,
            });

        var str1 = query.ToQueryString();

        _output.WriteLine(str1);

        var result = query.ToArray();


    }

    [Fact]
    public void TestRightOuterJoin()
    {
        using var ctx = _fixture.CreateDbContext();

        var query = ctx.Departments.RightOuterJoin(ctx.Employees,
            z => z.ID,
            z => z.DepartmentID,
            (department, employee) => new
            {
                Department = department.Name,
                Employee = employee.Name,
            });

        var str1 = query.ToQueryString();

        _output.WriteLine(str1);

        var result = query.ToArray();
    }

    [Fact]
    public async Task TestOuterJoin()
    {
        await using var ctx = _fixture.CreateDbContext();

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

        var str1 = query.ToQueryString();

        _output.WriteLine(str1);


        query = ctx.Departments.OuterJoin(ctx.Employees,
            z => z.ID,
            z => z.DepartmentID,
            (department, employee) => new
            {
                Department = department.Name,
                Employee = employee.Name,
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
}