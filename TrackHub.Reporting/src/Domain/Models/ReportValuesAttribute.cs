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

namespace TrackHub.Reporting.Domain.Models;

// Marks a row column whose values come from a fixed vocabulary, so every output renders them
// through the resx entries "{Vocabulary}_{Value}" instead of the stored value.
[AttributeUsage(AttributeTargets.Property)]
public sealed class ReportValuesAttribute(string vocabulary) : Attribute
{
    public string Vocabulary { get; } = vocabulary;
}
