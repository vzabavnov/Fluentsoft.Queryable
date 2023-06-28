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

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fluentsoft.System.Linq.Queryable.Tests;

[EntityTypeConfiguration(typeof(EmployeeConfig))]
public class Employee
{
    public int ID { get; set; }

    public string Name { get; set; } = null!;

    public int? DepartmentID { get; set; }

    public Department? Department { get; set;}
}

public class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(z => z.ID);

        builder.HasIndex(z => z.Name).IsUnique(false);

        builder.Property(z => z.ID).ValueGeneratedOnAdd();

        builder.Property(z => z.DepartmentID).HasColumnName("department_id");

        builder.Property(z => z.Name).HasMaxLength(128);

        builder.HasOne(z => z.Department)
            .WithMany(z => z.Employees)
            .HasForeignKey(z => z.DepartmentID);
    }
}