using System.Text.Json;
using Microsoft.Extensions.Options;
using SalesAnalysis.Etl.Worker.Options;

namespace SalesAnalysis.Etl.Worker.Staging;

public sealed class JsonStagingWriter : IStagingWriter
{
    private readonly StagingOptions _options;
    private readonly ILogger<JsonStagingWriter> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonStagingWriter(IOptions<StagingOptions> options, ILogger<JsonStagingWriter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task WriteAsync<T>(string fileName, IReadOnlyCollection<T> records, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_options.OutputPath);
        var path = Path.Combine(_options.OutputPath, fileName);

        _logger.LogInformation("Escribiendo staging {FileName} con {Count} registros", fileName, records.Count);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(records, JsonOptions), cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Staging escrito en {Path}", path);
    }
}
