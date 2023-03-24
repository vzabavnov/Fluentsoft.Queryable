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
using Xunit;
using Xunit.Abstractions;

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
    public async Task CheckOuterJoin()
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

        _output.WriteLine(query.ToQueryString());


        query = ctx.Departments.OuterJoin(ctx.Employees,
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
}