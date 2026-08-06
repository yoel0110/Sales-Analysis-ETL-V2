using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;

namespace SalesAnalysis.Etl.Worker.Extractors;

public sealed class ApiCustomerExtractor : IExtractor<ApiCustomerRecord>
{
    private readonly HttpClient _httpClient;
    private readonly ApiSourceOptions _options;
    private readonly ILogger<ApiCustomerExtractor> _logger;

    public string SourceName => "API Customers";

    public ApiCustomerExtractor(HttpClient httpClient, IOptions<ApiSourceOptions> options, ILogger<ApiCustomerExtractor> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<ApiCustomerRecord>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extrayendo {Source} desde {Url}", SourceName, _options.CustomersUrl);

        var response = await _httpClient.GetAsync(_options.CustomersUrl, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var records = await response.Content
            .ReadFromJsonAsync<List<ApiCustomerRecord>>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var result = records ?? new List<ApiCustomerRecord>();
        _logger.LogInformation("{Source}: {Count} registros extraidos", SourceName, result.Count);
        return result;
    }
}
