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

using Common.Application.GraphQL.Inputs;
using Common.Domain.Helpers;

namespace Common.Application.Extensions;

public static class FiltersExtensions
{
    public static Filters GetFilters(this FiltersInput filtersInput) 
    {
        var filtersDictionary = filtersInput.Filters.ToDictionary(f => f.Key, f => f.Value);
        return new Filters(filtersDictionary);
    }
}
