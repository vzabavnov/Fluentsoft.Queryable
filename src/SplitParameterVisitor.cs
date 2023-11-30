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

using System.Collections.ObjectModel;
using System.Reflection;

namespace Fluentsoft.System.Linq;

internal abstract class SplitParameterVisitor<T, TResult>(IReadOnlyDictionary<ParameterExpression, MemberExpression> map,
    Expression expressionBody) : ExpressionVisitor
{
    protected static ParameterExpression _parameterExpression = Expression.Parameter(typeof(T));
    protected readonly Expression _expressionBody = expressionBody;

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return map.TryGetValue(node, out var arg) ? arg : base.VisitParameter(node);
    }

    public Expression<Func<T, TResult>> Translate()
    {
        return Expression.Lambda<Func<T, TResult>>(base.Visit(_expressionBody), _parameterExpression);
    }

    internal static string[] GetPropertyNames(Type sourceType, Type[] typesToSeek)
    {
        if (typesToSeek.Length == 0)
        {
            throw new ArgumentException("at least one type is required", nameof(typesToSeek));
        }

        var names = new string[typesToSeek.Length];

        var properties = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

        var idx = 0;
        foreach (var t in properties)
        {
            if (typesToSeek[idx] == t.PropertyType)
            {
                names[idx] = t.Name;
                idx++;

                if (idx == typesToSeek.Length)
                {
                    break;
                }
            }
        }

        if (idx < typesToSeek.Length)
        {
            throw new ArgumentException("the give type does not contains enough properties for specified types");
        }

        return names;
    }

    internal static Dictionary<ParameterExpression, MemberExpression> GetParameterMap(ParameterExpression parameterExpression,
        ReadOnlyCollection<ParameterExpression> parameters)
    {
        var types = parameters.Select(z => z.Type).ToArray();
        var names = GetPropertyNames(parameterExpression.Type, types);

        return parameters.Zip(names).ToDictionary(z => z.First, z => Expression.Property(parameterExpression, z.Second));
    }
}

internal class SplitParameterVisitor<TArg1, TArg2, T, TResult>
    (Expression<Func<TArg1, TArg2, TResult>> expression) : SplitParameterVisitor<T, TResult>(
        GetParameterMap(_parameterExpression, expression.Parameters),
        expression.Body);

internal class SplitParameterVisitor<TArg1, TArg2, TArg3, T, TResult>
    (Expression<Func<TArg1, TArg2, TArg3, TResult>> expression) : SplitParameterVisitor<T, TResult>(
        GetParameterMap(_parameterExpression, expression.Parameters),
        expression.Body);

internal class SplitParameterVisitor<TArg1, TArg2, TArg3, TArg4, T, TResult>
    (Expression<Func<TArg1, TArg2, TArg3, TArg4, TResult>> expression) : SplitParameterVisitor<T, TResult>(
        GetParameterMap(_parameterExpression, expression.Parameters),
        expression.Body);

internal class SplitParameterVisitor<TArg1, TArg2, TArg3, TArg4, TArg5, T, TResult>
    (Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> expression) : SplitParameterVisitor<T, TResult>(
        GetParameterMap(_parameterExpression, expression.Parameters),
        expression.Body);

internal class SplitParameterVisitor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, T, TResult>
    (Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> expression) : SplitParameterVisitor<T, TResult>(
        GetParameterMap(_parameterExpression, expression.Parameters),
        expression.Body);

internal class SplitParameterVisitor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, T, TResult>(
    Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>> expression) : SplitParameterVisitor<T, TResult>(
    GetParameterMap(_parameterExpression, expression.Parameters),
    expression.Body);

internal class SplitParameterVisitor<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, T, TResult>(
    Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>> expression) : SplitParameterVisitor<T, TResult>(
    GetParameterMap(_parameterExpression, expression.Parameters),
    expression.Body);