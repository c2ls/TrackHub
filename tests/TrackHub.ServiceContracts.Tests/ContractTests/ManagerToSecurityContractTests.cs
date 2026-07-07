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
using TrackHub.Manager.Infrastructure.SecurityApi;
using TrackHub.ServiceContracts.Tests.Harness;

namespace TrackHub.ServiceContracts.Tests.ContractTests;

// Manager's identity-provisioning call into Security — the
// createManager mutation on the account/user-creation path.
[TestFixture]
public class ManagerToSecurityContractTests
{
    private static readonly DocumentValidator Validator = DocumentValidatorBuilder.New().AddDefaultRules().Build();
    private ISchemaDefinition _schema = null!;

    [OneTimeSetUp]
    public async Task BuildSecuritySchema() => _schema = await ProducerSchema.BuildSecuritySchemaAsync();

    [Test]
    public void CreateManagerMutation_IsValidAgainstSecuritySchema()
    {
        var document = Utf8GraphQLParser.Parse(SecurityWriter.CreateManagerMutation);
        var result = Validator.Validate(_schema, document);

        Assert.That(result.HasErrors, Is.False,
            () => "Manager→Security SecurityWriter.CreateUser no longer matches the Security schema: "
                + string.Join("; ", result.Errors.Select(e => e.Message)));
    }
}
