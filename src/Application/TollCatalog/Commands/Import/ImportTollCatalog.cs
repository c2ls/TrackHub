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

using System.Globalization;

namespace TrackHub.TripManagement.Application.TollCatalog.Commands.Import;

/// <summary>
/// Bulk toll-catalog import from CSV: <c>stationCode, stationName, latitude, longitude, country,
/// region, roadName, direction, vehicleClassCode, amount, currency, effectiveFrom</c>.
/// <para>
/// <b>No partial-failure rollback</b> (the spec 08 geofence-import contract): a bad row is reported
/// with its row number and skipped, and every good row still lands. Rolling back a 4 000-row file
/// because row 3 121 has a malformed date would make the operator re-run the whole import to find
/// out row 3 122 is also wrong — the row-level report is what makes the feature usable.
/// </para>
/// NOT feature-flagged — platform reference data (spec 11 §3).
/// </summary>
[Authorize(Resource = Resources.TollCatalog, Action = Actions.Write)]
public readonly record struct ImportTollCatalogCommand(string Csv) : IRequest<TollCatalogImportResultVm>;

public sealed class ImportTollCatalogCommandHandler(ITollCatalogWriter writer)
    : IRequestHandler<ImportTollCatalogCommand, TollCatalogImportResultVm>
{
    private const int ExpectedColumns = 12;
    private const string InvalidRowCode = "TOLL_IMPORT_INVALID_ROW";

    public async Task<TollCatalogImportResultVm> Handle(ImportTollCatalogCommand request, CancellationToken cancellationToken)
    {
        var rows = new List<TollCatalogImportRowDto>();
        var errors = new List<TollCatalogImportErrorVm>();

        var lines = request.Csv.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var rowNumber = 0;
        var rowsRead = 0;

        foreach (var line in lines)
        {
            rowNumber++;

            // Header row, recognised by its first column name rather than by position.
            if (rowNumber == 1 && line.StartsWith("stationCode", StringComparison.OrdinalIgnoreCase))
                continue;

            rowsRead++;
            var fields = line.Split(',').Select(f => f.Trim()).ToArray();

            if (fields.Length < ExpectedColumns)
            {
                errors.Add(new TollCatalogImportErrorVm(rowNumber, InvalidRowCode, $"Expected {ExpectedColumns} columns, found {fields.Length}."));
                continue;
            }

            var parsed = Parse(rowNumber, fields, out var error);
            if (error is { } failure)
            {
                errors.Add(failure);
                continue;
            }

            rows.Add(parsed);
        }

        var result = rows.Count == 0
            ? new TollCatalogImportResultVm(0, 0, 0, 0, [])
            : await writer.ImportAsync(rows, cancellationToken);

        // Parse errors and writer errors are one report to the operator, not two.
        return new TollCatalogImportResultVm(
            rowsRead,
            result.StationsCreated,
            result.StationsUpdated,
            result.TariffsCreated,
            [.. errors, .. result.Errors]);
    }

    private static TollCatalogImportRowDto Parse(int rowNumber, string[] fields, out TollCatalogImportErrorVm? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(fields[1]))
        {
            error = new TollCatalogImportErrorVm(rowNumber, InvalidRowCode, "stationName is required.");
            return default;
        }

        if (!double.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) || latitude is < -90d or > 90d)
        {
            error = new TollCatalogImportErrorVm(rowNumber, InvalidRowCode, "latitude is not a valid coordinate.");
            return default;
        }

        if (!double.TryParse(fields[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude) || longitude is < -180d or > 180d)
        {
            error = new TollCatalogImportErrorVm(rowNumber, InvalidRowCode, "longitude is not a valid coordinate.");
            return default;
        }

        decimal? amount = null;
        if (!string.IsNullOrWhiteSpace(fields[9]))
        {
            if (!decimal.TryParse(fields[9], NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedAmount) || parsedAmount < 0m)
            {
                error = new TollCatalogImportErrorVm(rowNumber, InvalidRowCode, "amount is not a valid non-negative number.");
                return default;
            }

            amount = parsedAmount;
        }

        DateOnly? effectiveFrom = null;
        if (!string.IsNullOrWhiteSpace(fields[11]))
        {
            if (!DateOnly.TryParse(fields[11], CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                error = new TollCatalogImportErrorVm(rowNumber, InvalidRowCode, "effectiveFrom is not a valid date.");
                return default;
            }

            effectiveFrom = parsedDate;
        }

        // A priced row must say what it prices and in what currency, or the estimate it feeds
        // would be unreproducible.
        if (amount.HasValue && (string.IsNullOrWhiteSpace(fields[8]) || string.IsNullOrWhiteSpace(fields[10]) || effectiveFrom is null))
        {
            error = new TollCatalogImportErrorVm(rowNumber, InvalidRowCode, "vehicleClassCode, currency and effectiveFrom are required when an amount is given.");
            return default;
        }

        return new TollCatalogImportRowDto(
            rowNumber,
            NullIfEmpty(fields[0]),
            fields[1],
            latitude,
            longitude,
            NullIfEmpty(fields[4]),
            NullIfEmpty(fields[5]),
            NullIfEmpty(fields[6]),
            NullIfEmpty(fields[7]),
            NullIfEmpty(fields[8]),
            amount,
            NullIfEmpty(fields[10]),
            effectiveFrom);
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;
}

public sealed class ImportTollCatalogValidator : AbstractValidator<ImportTollCatalogCommand>
{
    public ImportTollCatalogValidator()
        => RuleFor(v => v.Csv).NotEmpty();
}
