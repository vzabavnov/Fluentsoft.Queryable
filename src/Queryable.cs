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

public static class Queryable
{
    /// <summary>
    ///     Correlates all records from the left table, and the matching records from the right table based on matching keys
    /// </summary>
    /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="outer">
    ///     <see cref="IQueryable{TOuter}" />
    ///     The first sequence to join.
    /// </param>
    /// <param name="inner">
    ///     <see cref="IEnumerable{TInner}" />
    ///     The sequence to join to the first sequence.
    /// </param>
    /// <param name="outerKey">
    ///     <see cref="Expression{T}" /> of <see cref="Func{TOuter, TKey}" />
    ///     An expression to extract the join key from each element of the first sequence.
    /// </param>
    /// <param name="innerKey">
    ///     <see cref="Expression{T}" /> of <see cref="Func{TOuter, TKey}" />
    ///     An expression to extract the join key from each element of the second sequence.
    /// </param>
    /// <param name="resultSelector">
    ///     <see cref="Expression{T}" /> of <see cref="Func{TOuter, TOuter, TResult}" />
    ///     An expression to create a result element from two matching elements.
    /// </param>
    public static IQueryable<TResult> LeftOuterJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer,
        IEnumerable<TInner> inner,
        Expression<Func<TOuter, TKey>> outerKey,
        Expression<Func<TInner, TKey>> innerKey,
        Expression<Func<TOuter, TInner?, TResult>> resultSelector)
    {
        return outer.GroupJoin(inner,
                outerKey,
                innerKey,
                (o, i) => new
                {
                    o,
                    i
                })
            .SelectMany(z => z.i.DefaultIfEmpty(),
                (t, i) => new
                {
                    t.o,
                    i
                })
            .Select(resultSelector);
    }

    public static IQueryable<TResult> RightOuterJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer,
        IQueryable<TInner> inner,
        Expression<Func<TOuter, TKey>> outerKey,
        Expression<Func<TInner, TKey>> innerKey,
        Expression<Func<TOuter?, TInner, TResult>> resultSelector)
    {
        return inner.GroupJoin(outer,
                innerKey,
                outerKey,
                (o, i) => new
                {
                    o,
                    i
                })
            .SelectMany(z => z.i.DefaultIfEmpty(),
                (t, i) => new
                {
                    t.o,
                    i
                })
            .Select(resultSelector.SwitchParameters());
    }

    public static IQueryable<TResult> FullOuterJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer,
        IQueryable<TInner> inner,
        Expression<Func<TOuter, TKey>> outerKey,
        Expression<Func<TInner, TKey>> innerKey,
        Expression<Func<TOuter?, TInner?, TResult>> resultSelector)
    {
        return outer.GroupJoin(inner,
                outerKey,
                innerKey,
                (o, i) => new
                {
                    o,
                    i
                })
            .SelectMany(z => z.i.DefaultIfEmpty(),
                (t, i) => new
                {
                    t.o,
                    i
                })
            .Select(resultSelector)
            .Union(inner.GroupJoin(outer,
                    innerKey,
                    outerKey,
                    (o, i) => new
                    {
                        o,
                        i
                    })
                .SelectMany(z => z.i.DefaultIfEmpty(),
                    (t, i) => new
                    {
                        t.o,
                        i
                    })
                .Select(resultSelector.SwitchParameters()));
    }

    /// <summary>
    /// Projects each element of a sequence into a new form.
    /// </summary>
    public static IQueryable<TResult> Select<T, TResult, T1, T2>(this IQueryable<T> source, Expression<Func<T1, T2, TResult>> selector)
    {
        return source.Select(selector.SplitParameters<T1, T2, T, TResult>());
    }

    /// <summary>
    /// Projects each element of a sequence into a new form.
    /// </summary>
    public static IQueryable<TResult> Select<T, TResult, T1, T2, T3>(this IQueryable<T> source, Expression<Func<T1, T2, T3, TResult>> selector)
    {
        return source.Select(selector.SplitParameters<T1, T2, T3, T, TResult>());
    }

    /// <summary>
    /// Filters a sequence of values based on a predicate with three arguments taken from source value by splitting.
    /// </summary>
    public static IQueryable<T> Where<T, T1, T2, T3>(this IQueryable<T> source, Expression<Func<T1, T2, T3, bool>> predicate)
    {
        return source.Where(predicate.SplitParameters<T1, T2, T3, T, bool>());
    }

    /// <summary>
    /// Filters a sequence of values based on a predicate with two arguments taken from source value by splitting.
    /// </summary>
    public static IQueryable<T> Where<T, T1, T2>(this IQueryable<T> source, Expression<Func<T1, T2, bool>> predicate)
    {
        return source.Where(predicate.SplitParameters<T1, T2, T, bool>());
    }
}