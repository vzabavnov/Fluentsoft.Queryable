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

namespace Fluentsoft.System.Linq;

internal abstract class SplitParameterVisitor<T, TResult> : ExpressionVisitor
{
    private readonly PropertyInfo[] _properties;
    protected ParameterExpression _parameterExpression;

    protected SplitParameterVisitor()
    {
        var type = typeof(T);
        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;
        _properties = type.GetProperties(FLAGS);

        _parameterExpression = Expression.Parameter(type);
    }

    protected abstract Expression GetExpressionBody();

    public Expression<Func<T, TResult>> Translate()
    {
        return Expression.Lambda<Func<T, TResult>>(base.Visit(GetExpressionBody()), _parameterExpression);
    }

    protected string GetProperty<TArg>(int skip)
    {
#if NETCOREAPP
        var name = _properties.Where(z => z.PropertyType.IsAssignableTo(typeof(TArg))).Skip(skip).Select(z => z.Name).FirstOrDefault();
#else
        var name = _properties.Where(z => typeof(TArg).IsAssignableFrom(z.PropertyType)).Skip(skip).Select(z => z.Name).FirstOrDefault();
#endif

        return name ?? throw new ArgumentException($"Cannot find property in type {typeof(T).Name} of type {typeof(TArg).Name}");
    }
}

internal class SplitParameterVisitor<TArg1, TArg2, T, TResult> : SplitParameterVisitor<T, TResult>
{
    private readonly MemberExpression _arg1Expression;
    private readonly MemberExpression _arg2Expression;
    private readonly Expression<Func<TArg1, TArg2, TResult>> _expression;
    private readonly ParameterExpression _p1;
    private readonly ParameterExpression _p2;

    public SplitParameterVisitor(Expression<Func<TArg1, TArg2, TResult>> expression)
    {
        _expression = expression;

        _p1 = expression.Parameters[0];
        _p2 = expression.Parameters[1];

        var prop1Name = GetProperty<TArg1>(0);
        var prop2Name = GetProperty<TArg2>(typeof(TArg1) == typeof(TArg2) ? 1 : 0);

        _arg1Expression = Expression.Property(_parameterExpression, prop1Name);
        _arg2Expression = Expression.Property(_parameterExpression, prop2Name);
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

    protected override Expression GetExpressionBody()
    {
        return _expression.Body;
    }
}

internal class SplitParameterVisitor<TArg1, TArg2, TArg3, T, TResult> : SplitParameterVisitor<T, TResult>
{
    private readonly MemberExpression _arg1Expression;
    private readonly MemberExpression _arg2Expression;
    private readonly MemberExpression _arg3Expression;
    private readonly Expression<Func<TArg1, TArg2, TArg3, TResult>> _expression;
    private readonly ParameterExpression _p1;
    private readonly ParameterExpression _p2;
    private readonly ParameterExpression _p3;

    public SplitParameterVisitor(Expression<Func<TArg1, TArg2, TArg3, TResult>> expression)
    {
        _expression = expression;

        _p1 = expression.Parameters[0];
        _p2 = expression.Parameters[1];
        _p3 = expression.Parameters[2];

        var prop1Name = GetProperty<TArg1>(0);
        var prop2Name = GetProperty<TArg2>(typeof(TArg1) == typeof(TArg2) ? 1 : 0);

        int skip;
        if (typeof(TArg1) == typeof(TArg2))
        {
            if (typeof(TArg1) == typeof(TArg3))
            {
                skip = 2;
            }
            else if (typeof(TArg1) == typeof(TArg3) || typeof(TArg2) == typeof(TArg3))
            {
                skip = 1;
            }
            else
            {
                skip = 0;
            }
        }
        else if (typeof(TArg1) == typeof(TArg3) || typeof(TArg2) == typeof(TArg3))
        {
            skip = 1;
        }
        else
        {
            skip = 0;
        }

        var prop3Name = GetProperty<TArg3>(skip);

        _arg1Expression = Expression.Property(_parameterExpression, prop1Name);
        _arg2Expression = Expression.Property(_parameterExpression, prop2Name);
        _arg3Expression = Expression.Property(_parameterExpression, prop3Name);
    }

    protected override Expression GetExpressionBody()
    {
        return _expression.Body;
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

        if (node == _p3)
        {
            return _arg3Expression;
        }

        return base.VisitParameter(node);
    }
}