using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using SalesAnalysis.Etl.Worker.Extractors.Mappings;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;

namespace SalesAnalysis.Etl.Worker.Extractors;

public sealed class CsvProductExtractor : IExtractor<CsvProductRecord>
{
    private readonly CsvSourceOptions _options;
    private readonly ILogger<CsvProductExtractor> _logger;

    public string SourceName => "CSV Products";

    public CsvProductExtractor(IOptions<CsvSourceOptions> options, ILogger<CsvProductExtractor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<CsvProductRecord>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_options.InputPath, "products.csv");
        _logger.LogInformation("Extrayendo {Source} desde {Path}", SourceName, path);

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        csv.Context.RegisterClassMap<CsvProductRecordMap>();
        var records = new List<CsvProductRecord>();
        await foreach (var record in csv.GetRecordsAsync<CsvProductRecord>(cancellationToken).ConfigureAwait(false))
        {
            records.Add(record);
        }

        _logger.LogInformation("{Source}: {Count} registros extraidos", SourceName, records.Count);
        return records;
    }
}
