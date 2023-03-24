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

using System.Reflection;

namespace FSC.System.Linq;

public static class SplitParameter<T1, T2>
{
    public static Expression<Func<T, TResult>> Translate<T, TResult>(Expression<Func<T1, T2, TResult>> expression)
    {
        return new SplitParameterVisitor<T, TResult>(expression).Translate();
    }

    public static IQueryable<TResult> Select<T, TResult>(IQueryable<T> source, Expression<Func<T1, T2, TResult>> selector)
    {
        return source.Select(Translate<T, TResult>(selector));
    }

    public static IQueryable<T> Where<T>(IQueryable<T> source, Expression<Func<T1, T2, bool>> predicate)
    {
        return source.Where(Translate<T, bool>(predicate));
    }

    internal class SplitParameterVisitor<T, TResult> : ExpressionVisitor
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
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;
            var properties = type.GetProperties(FLAGS).ToArray();

            var prop1Name = properties.Where(z => z.PropertyType.IsAssignableTo(typeof(T1))).Select(z => z.Name).FirstOrDefault();

            var prop2Name = typeof(T1) == typeof(T2)
                ? properties.Where(z => z.PropertyType.IsAssignableTo(typeof(T2)))
                    .Skip(1)
                    .Select(z => z.Name).FirstOrDefault()
                : properties.Where(z => z.PropertyType.IsAssignableTo(typeof(T2)))
                    .Select(z => z.Name).FirstOrDefault();

            if (prop1Name == null)
            {
                throw new ArgumentException($"Cannot find property in type {type.Name} of type {typeof(T1).Name}");
            }

            if (prop2Name == null)
            {
                throw new ArgumentException($"Cannot find property in type {type.Name} of type {typeof(T2).Name}");
            }

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
}