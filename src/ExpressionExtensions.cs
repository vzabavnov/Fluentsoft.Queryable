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

namespace Fluentsoft.System.Linq;

/// <summary>
/// Contains extension methods related to the <see cref="Expression"/>
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Translate the <paramref name="expression"/> to result expression by switch arguments
    /// </summary>
    /// <typeparam name="T1">The type of first argument</typeparam>
    /// <typeparam name="T2">The type of second argument</typeparam>
    /// <typeparam name="TResult">The type of expression's result</typeparam>
    /// <param name="expression">The expression to translate</param>
    /// <returns>The expression where first argument is second argument from source expression
    /// and second argument is first argument from source expression</returns>
    public static Expression<Func<T2, T1, TResult>> SwitchParameters<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression)
    {
        return Expression.Lambda<Func<T2, T1, TResult>>(expression.Body, expression.Parameters[1], expression.Parameters[0]);
    }

    /// <summary>
    /// Translate source <paramref name="expression"/> with two arguments to expression with type where arguments are property of that type
    /// </summary>
    /// <typeparam name="T1">The type of first argument</typeparam>
    /// <typeparam name="T2">The type of second argument</typeparam>
    /// <typeparam name="T">The type of argument of resulting expression. it must have at least two properties of types <typeparamref name="T1"/> and <typeparamref name="T2"/></typeparam>
    /// <typeparam name="TResult">The type of expression's result</typeparam>
    /// <param name="expression">The expression with one argument of type <typeparamref name="T"/></param>
    /// <returns>The expression with one argument of type <typeparamref name="T"/>.</returns>
    public static Expression<Func<T, TResult>> SplitParameters<T1, T2, T, TResult>(this Expression<Func<T1, T2, TResult>> expression)
    {
        return new SplitParameterVisitor<T1, T2, T, TResult>(expression).Translate();
    }

    /// <summary>
    /// Translate source <paramref name="expression"/> with two arguments to expression with type where arguments are property of that type
    /// </summary>
    /// <typeparam name="T1">The type of first argument</typeparam>
    /// <typeparam name="T2">The type of second argument</typeparam>
    /// <typeparam name="T3">The type of third argument</typeparam>
    /// <typeparam name="T">The type of argument of resulting expression. it must have at least two properties of types
    /// <typeparamref name="T1"/>, <typeparamref name="T2"/> and <typeparamref name="T3"/></typeparam>
    /// <typeparam name="TResult">The type of expression's result</typeparam>
    /// <param name="expression">The expression with one argument of type <typeparamref name="T"/></param>
    /// <returns>The expression with one argument of type <typeparamref name="T"/>.</returns>
    public static Expression<Func<T, TResult>> SplitParameters<T1, T2, T3, T, TResult>(this Expression<Func<T1, T2, T3, TResult>> expression)
    {
        return new SplitParameterVisitor<T1, T2, T3, T, TResult>(expression).Translate();
    }
}