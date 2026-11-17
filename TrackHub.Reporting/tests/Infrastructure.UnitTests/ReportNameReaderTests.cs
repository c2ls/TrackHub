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

using System.Text.Json;
using GraphQL;
using GraphQL.Client.Abstractions;
using Moq;
using Common.Application.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Infrastructure.GraphQLApi;

namespace TrackHub.Reporting.Infrastructure.UnitTests;

[TestFixture]
public class ReportNameReaderTests
{
    private static readonly Guid ActiveUnit = Guid.NewGuid();
    private static readonly Guid KnownDocument = Guid.NewGuid();
    private static readonly Guid DeniedDocument = Guid.NewGuid();

    private ReportNameReader _reader = null!;
    private List<string> _queries = null!;

    [SetUp]
    public void SetUp()
    {
        _queries = [];
        var client = new Mock<IGraphQLClient>();
        client.Setup(c => c.SendQueryAsync<object>(It.IsAny<GraphQLRequest>(), It.IsAny<CancellationToken>()))
            .Returns((GraphQLRequest request, CancellationToken _) =>
            {
                _queries.Add(request.Query!);
                return Task.FromResult(Respond(request));
            });
        var factory = new Mock<IGraphQLClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client.Object);
        _reader = new ReportNameReader(factory.Object, Mock.Of<IGpsManagerReader>(), Mock.Of<IWorkforceReportReader>());
    }

    private static GraphQLResponse<object> Respond(GraphQLRequest request)
    {
        if (request.Query!.Contains("retiredTransporters") || Variable(request, "id") == DeniedDocument)
        {
            return new GraphQLResponse<object> { Errors = [new GraphQLError { Message = "Forbidden" }] };
        }

        var json = request.Query!.Contains("transportersByAccount")
            ? $$$"""{"transportersByAccount":{"items":[{"transporterId":"{{{ActiveUnit}}}","name":"AAA000"}],"totalCount":1}}"""
            : """{"document":{"title":"","fileName":"soat.pdf"}}""";
        return new GraphQLResponse<object> { Data = JsonDocument.Parse(json).RootElement.Clone() };
    }

    private static Guid? Variable(GraphQLRequest request, string name)
        => request.Variables?.GetType().GetProperty(name)?.GetValue(request.Variables) as Guid?;

    [Test]
    public async Task TransporterNames_KeepWhatTheCallerMayRead_WhenRetiredUnitsAreDenied()
    {
        var names = await _reader.GetTransporterNamesAsync([ActiveUnit, Guid.NewGuid()], CancellationToken.None);

        Assert.That(names, Is.EqualTo(new Dictionary<Guid, string> { [ActiveUnit] = "AAA000" }));
        Assert.That(_queries.Count(q => q.Contains("retiredTransporters")), Is.EqualTo(1));
    }

    [Test]
    public async Task TransporterNames_SkipRetiredUnits_WhenEveryIdIsActive()
    {
        var names = await _reader.GetTransporterNamesAsync([ActiveUnit], CancellationToken.None);

        Assert.That(names, Is.EqualTo(new Dictionary<Guid, string> { [ActiveUnit] = "AAA000" }));
        Assert.That(_queries.Any(q => q.Contains("retiredTransporters")), Is.False);
    }

    [Test]
    public async Task DocumentNames_FallBackToTheFileName_AndSkipDeniedDocuments()
    {
        var names = await _reader.GetDocumentNamesAsync([KnownDocument, DeniedDocument, KnownDocument], CancellationToken.None);

        Assert.That(names, Is.EqualTo(new Dictionary<Guid, string> { [KnownDocument] = "soat.pdf" }));
    }
}
