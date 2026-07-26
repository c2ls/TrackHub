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

using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using TrackHub.Telemetry.Web.GraphQL.Mutation;
using TrackHub.Telemetry.Web.GraphQL.Query;

namespace TrackHub.Telemetry.Application.UnitTests;

// Guards the GraphQL contract the Router (TelemetryApi) and Reporting depend on: the exposed field
// names must exactly match the operations those clients send. This builds the real schema from the
// Web layer's Query/Mutation types (no host, no DB), so a rename or a lost/added field fails here.
[TestFixture]
public class SchemaContractTests
{
    private static readonly string[] ExpectedQueries =
    [
        "transporterPositionByOperator", "transporterPositionsByOperators", "positionHistory", "positionHistoryRange",
        "operatorSyncRuns", "operatorHealth", "operatorHealthHistory", "operatorHealthSummary",
        "platformSyncActivity",
    ];

    private static readonly string[] ExpectedMutations =
    [
        "bulkTransporterPosition", "appendPositionHistory", "appendPositionHistoryBatch",
        "persistResolvedAddress", "recordOperatorSyncRun", "recordOperatorHealth",
    ];

    [Test]
    public async Task Schema_ExposesExactlyTheExpectedTelemetryOperations()
    {
        var schema = await new ServiceCollection()
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .BuildSchemaAsync();

        var queryFields = schema.QueryType.Fields
            .Where(f => !f.Name.StartsWith("__")).Select(f => f.Name).ToHashSet();
        var mutationFields = schema.MutationType!.Fields
            .Where(f => !f.Name.StartsWith("__")).Select(f => f.Name).ToHashSet();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(queryFields, Is.EquivalentTo(ExpectedQueries), "Telemetry query fields must match the contract set the clients call");
            Assert.That(mutationFields, Is.EquivalentTo(ExpectedMutations), "Telemetry mutation fields must match the contract set the clients call");
        }
    }
}
