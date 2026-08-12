// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Linq.Expressions;

namespace Common.Domain.Helpers;

public class Filters(Dictionary<string, object> filters)
{
    private readonly Dictionary<string, object> _filters = filters ?? [];

    public IQueryable<T> Apply<T>(IQueryable<T> query)
    {
        foreach (var filter in _filters)
        {
            query = ApplyFilter(query, filter.Key, filter.Value);
        }
        return query;
    }

    private static IQueryable<T> ApplyFilter<T>(IQueryable<T> query, string propertyName, object value)
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, propertyName);
        var constant = Expression.Constant(value);
        var equal = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

        return query.Where(lambda);
    }
}
