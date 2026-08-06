using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using SalesAnalysis.Etl.Worker.Extractors.Mappings;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;

namespace SalesAnalysis.Etl.Worker.Extractors;

public sealed class CsvCustomerExtractor : IExtractor<CsvCustomerRecord>
{
    private readonly CsvSourceOptions _options;
    private readonly ILogger<CsvCustomerExtractor> _logger;

    public string SourceName => "CSV Customers";

    public CsvCustomerExtractor(IOptions<CsvSourceOptions> options, ILogger<CsvCustomerExtractor> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<CsvCustomerRecord>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_options.InputPath, "customers.csv");
        _logger.LogInformation("Extrayendo {Source} desde {Path}", SourceName, path);

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        csv.Context.RegisterClassMap<CsvCustomerRecordMap>();
        var records = new List<CsvCustomerRecord>();
        await foreach (var record in csv.GetRecordsAsync<CsvCustomerRecord>(cancellationToken).ConfigureAwait(false))
        {
            records.Add(record);
        }

        _logger.LogInformation("{Source}: {Count} registros extraidos", SourceName, records.Count);
        return records;
    }
}
