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

namespace FSC.System.Linq;

public static class ExpressionExtensions
{
    public static Expression<Func<T2, T1, TResult>> SwitchParameters<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression)
    {
        return Expression.Lambda<Func<T2, T1, TResult>>(expression.Body, expression.Parameters[1], expression.Parameters[0]);
    }

    public static Expression<Func<T, TResult>> SplitParameters<T1, T2, T, TResult>(this Expression<Func<T1, T2, TResult>> expression)
    {
        return new SplitParameterVisitor<T1, T2, T, TResult>(expression).Translate();
    }
}