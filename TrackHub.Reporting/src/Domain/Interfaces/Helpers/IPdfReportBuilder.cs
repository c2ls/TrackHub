using System.Globalization;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Interfaces.Helpers;

// Renders a ReportDataset to a branded, localized, paginated tabular PDF.
public interface IPdfReportBuilder
{
    byte[] Build(ReportDataset dataset, CultureInfo culture);
}
