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

using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Validation;
using Microsoft.Extensions.DependencyInjection;
using TrackHub.Security.Infrastructure.ManagerApi;
using TrackHub.ServiceContracts.Tests.Harness;

namespace TrackHub.ServiceContracts.Tests.ContractTests;

// Security's user-replication mutations into Manager. Simple CRUD by
// shape, but they keep the two user stores in sync — a silent drift here desynchronizes
// identity between services.
[TestFixture]
public class SecurityToManagerContractTests
{
    private static readonly DocumentValidator Validator = DocumentValidatorBuilder.New().AddDefaultRules().Build();
    private ISchemaDefinition _schema = null!;

    [OneTimeSetUp]
    public async Task BuildManagerSchema() => _schema = await ProducerSchema.BuildManagerSchemaAsync();

    private static IEnumerable<TestCaseData> Calls()
    {
        yield return new TestCaseData("ManagerWriter.CreateUser", ManagerWriter.CreateUserMutation);
        yield return new TestCaseData("ManagerWriter.UpdateUser", ManagerWriter.UpdateUserMutation);
        yield return new TestCaseData("ManagerWriter.DeleteUser", ManagerWriter.DeleteUserMutation);
        yield return new TestCaseData("ManagerAuditWriter.CreateAuditEvent", ManagerAuditWriter.CreateAuditEventMutation);
    }

    [TestCaseSource(nameof(Calls))]
    public void ProductionMutation_IsValidAgainstManagerSchema(string call, string query)
    {
        var document = Utf8GraphQLParser.Parse(query);
        var result = Validator.Validate(_schema, document);

        Assert.That(result.HasErrors, Is.False,
            () => $"Security→Manager {call} no longer matches the Manager schema: "
                + string.Join("; ", result.Errors.Select(e => e.Message)));
    }
}
