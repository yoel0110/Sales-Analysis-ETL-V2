using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using SalesAnalysis.Etl.Worker.Extractors.Mappings;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;

namespace SalesAnalysis.Etl.Worker.Extractors;

public sealed class CsvOrderDetailExtractor : IExtractor<CsvOrderDetailRecord>
{
    private readonly CsvSourceOptions _options;
    private readonly ILogger<CsvOrderDetailExtractor> _logger;

    public string SourceName => "CSV Order Details";

    public CsvOrderDetailExtractor(IOptions<CsvSourceOptions> options, ILogger<CsvOrderDetailExtractor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<CsvOrderDetailRecord>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_options.InputPath, "order_details.csv");
        _logger.LogInformation("Extrayendo {Source} desde {Path}", SourceName, path);

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        csv.Context.RegisterClassMap<CsvOrderDetailRecordMap>();
        var records = new List<CsvOrderDetailRecord>();
        await foreach (var record in csv.GetRecordsAsync<CsvOrderDetailRecord>(cancellationToken).ConfigureAwait(false))
        {
            records.Add(record);
        }

        _logger.LogInformation("{Source}: {Count} registros extraidos", SourceName, records.Count);
        return records;
    }
}
