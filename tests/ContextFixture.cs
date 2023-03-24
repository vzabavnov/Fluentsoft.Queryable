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

using Microsoft.Data.Sqlite;

namespace FSC.EntityFrameworkCore.Expressions.Tests;

public class ContextFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<TestDbContext> _contextOptions;

    public ContextFixture()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options;

        using var context = new TestDbContext(_contextOptions);

        context.Database.EnsureCreated();

        InitData(context);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public TestDbContext CreateDbContext() => new(_contextOptions);

    public static void InitData(TestDbContext ctx)
    {
        var d1 = new Department
        {
            Name = "hr"
        };
        var d2 = new Department
        {
            Name = "dev"
        };
        var d3 = new Department
        {
            Name = "ba"
        };
        var d4 = new Department
        {
            Name = "ui"
        };

        ctx.Departments.AddRange(d1,d2,d3,d4);

        ctx.Employees.AddRange(
            new Employee{ Department = d1, Name="Bob"},
            new Employee{Department = d2, Name = "Tom"},
            new Employee{Department = d3,Name = "Anna"},
            new Employee{Department = d4, Name = "Sara"},
            new Employee{Department = d2, Name = "Jon"},
            new Employee{Name = "Mike"}
        );

        ctx.SaveChanges();
    }
}