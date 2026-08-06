using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;

namespace SalesAnalysis.Etl.Worker.Extractors;

public sealed class ApiProductExtractor : IExtractor<ApiProductRecord>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiSourceOptions _options;
    private readonly ILogger<ApiProductExtractor> _logger;

    public string SourceName => "API Products";

    public ApiProductExtractor(IHttpClientFactory httpClientFactory, IOptions<ApiSourceOptions> options, ILogger<ApiProductExtractor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<ApiProductRecord>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extrayendo {Source} desde {Url}", SourceName, _options.ProductsUrl);

        var httpClient = _httpClientFactory.CreateClient("ApiClient");
        var response = await httpClient.GetAsync(_options.ProductsUrl, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var records = await response.Content
            .ReadFromJsonAsync<List<ApiProductRecord>>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var result = records ?? new List<ApiProductRecord>();
        _logger.LogInformation("{Source}: {Count} registros extraidos", SourceName, result.Count);
        return result;
    }
}
