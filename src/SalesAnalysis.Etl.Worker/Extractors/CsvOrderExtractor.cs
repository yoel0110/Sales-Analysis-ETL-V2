using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using SalesAnalysis.Etl.Worker.Extractors.Mappings;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;

namespace SalesAnalysis.Etl.Worker.Extractors;

public sealed class CsvOrderExtractor : IExtractor<CsvOrderRecord>
{
    private readonly CsvSourceOptions _options;
    private readonly ILogger<CsvOrderExtractor> _logger;

    public string SourceName => "CSV Orders";

    public CsvOrderExtractor(IOptions<CsvSourceOptions> options, ILogger<CsvOrderExtractor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<CsvOrderRecord>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_options.InputPath, "orders.csv");
        _logger.LogInformation("Extrayendo {Source} desde {Path}", SourceName, path);

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        csv.Context.RegisterClassMap<CsvOrderRecordMap>();
        var records = new List<CsvOrderRecord>();
        await foreach (var record in csv.GetRecordsAsync<CsvOrderRecord>(cancellationToken).ConfigureAwait(false))
        {
            records.Add(record);
        }

        _logger.LogInformation("{Source}: {Count} registros extraidos", SourceName, records.Count);
        return records;
    }
}
