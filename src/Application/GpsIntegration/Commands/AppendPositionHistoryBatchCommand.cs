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

namespace TrackHub.Telemetry.Application.GpsIntegration.Commands;

// Batched history append used by the Router storing pipeline: one mutation per
// operator sync cycle instead of one per position. Idempotent per row.
//
// Gating matrix (spec 01.3 A7): history writes are intentionally DOUBLE-gated, unlike the core
// latest-position/health writes. (1) The RequireFeature attribute gates on gps.positionHistory and
// forbids the global service client (AllowGlobalServiceClient = false) so an account without the
// feature cannot store history. (2) The retention-policy check below is kept as fail-safe defense
// in depth for any account-scoped service client that slips past the attribute. Both are deliberate.
[Authorize(Resource = Resources.PositionHistory, Action = Actions.Write, PrincipalTypes = "ServiceClient")]
[RequireFeature(FeatureKeys.GpsPositionHistory, AllowGlobalServiceClient = false)]
public readonly record struct AppendPositionHistoryBatchCommand(Guid AccountId, IReadOnlyCollection<TransporterPositionHistoryDto> Positions) : IRequest<int>;

public class AppendPositionHistoryBatchCommandHandler(ITransporterPositionHistoryWriter writer, IPositionRetentionPolicyReader policyReader)
    : IRequestHandler<AppendPositionHistoryBatchCommand, int>
{
    public async Task<int> Handle(AppendPositionHistoryBatchCommand request, CancellationToken cancellationToken)
    {
        var policy = await policyReader.GetAsync(request.AccountId, cancellationToken);
        if (!policy.HistoryEnabled)
        {
            return 0;
        }

        var rows = request.Positions.Where(p => p.AccountId == request.AccountId).ToList();
        return rows.Count == 0 ? 0 : await writer.AppendRangeAsync(rows, cancellationToken);
    }
}

public sealed class AppendPositionHistoryBatchCommandValidator : AbstractValidator<AppendPositionHistoryBatchCommand>
{
    public AppendPositionHistoryBatchCommandValidator()
    {
        RuleFor(v => v.AccountId).NotEmpty();
        RuleFor(v => v.Positions).NotEmpty();
    }
}
