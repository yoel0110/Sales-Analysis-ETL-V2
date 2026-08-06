using System.Diagnostics;
using Microsoft.Extensions.Options;
using SalesAnalysis.Etl.Worker.Extractors;
using SalesAnalysis.Etl.Worker.Models;
using SalesAnalysis.Etl.Worker.Options;
using SalesAnalysis.Etl.Worker.Staging;

namespace SalesAnalysis.Etl.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IStagingWriter _stagingWriter;
    private readonly StagingOptions _stagingOptions;
    private readonly IExtractor<CsvCustomerRecord> _csvCustomerExtractor;
    private readonly IExtractor<CsvProductRecord> _csvProductExtractor;
    private readonly IExtractor<CsvOrderRecord> _csvOrderExtractor;
    private readonly IExtractor<CsvOrderDetailRecord> _csvOrderDetailExtractor;
    private readonly IExtractor<ApiCustomerRecord> _apiCustomerExtractor;
    private readonly IExtractor<ApiProductRecord> _apiProductExtractor;

    public Worker(
        ILogger<Worker> logger,
        IStagingWriter stagingWriter,
        IOptions<StagingOptions> stagingOptions,
        IExtractor<CsvCustomerRecord> csvCustomerExtractor,
        IExtractor<CsvProductRecord> csvProductExtractor,
        IExtractor<CsvOrderRecord> csvOrderExtractor,
        IExtractor<CsvOrderDetailRecord> csvOrderDetailExtractor,
        IExtractor<ApiCustomerRecord> apiCustomerExtractor,
        IExtractor<ApiProductRecord> apiProductExtractor)
    {
        _logger = logger;
        _stagingWriter = stagingWriter;
        _stagingOptions = stagingOptions.Value;
        _csvCustomerExtractor = csvCustomerExtractor;
        _csvProductExtractor = csvProductExtractor;
        _csvOrderExtractor = csvOrderExtractor;
        _csvOrderDetailExtractor = csvOrderDetailExtractor;
        _apiCustomerExtractor = apiCustomerExtractor;
        _apiProductExtractor = apiProductExtractor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Iniciando proceso de extraccion del ETL");

        Directory.CreateDirectory(_stagingOptions.OutputPath);

        await ExtractAndStageAsync(_csvCustomerExtractor, "csv_customers.json", stoppingToken);
        await ExtractAndStageAsync(_csvProductExtractor, "csv_products.json", stoppingToken);
        await ExtractAndStageAsync(_csvOrderExtractor, "csv_orders.json", stoppingToken);
        await ExtractAndStageAsync(_csvOrderDetailExtractor, "csv_order_details.json", stoppingToken);
        await ExtractAndStageAsync(_apiCustomerExtractor, "api_customers.json", stoppingToken);
        await ExtractAndStageAsync(_apiProductExtractor, "api_products.json", stoppingToken);

        stopwatch.Stop();
        _logger.LogInformation("Proceso de extraccion completado en {ElapsedMs} ms", stopwatch.ElapsedMilliseconds);
    }

    private async Task ExtractAndStageAsync<T>(IExtractor<T> extractor, string stagingFileName, CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var records = await extractor.ExtractAsync(cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            await _stagingWriter.WriteAsync(stagingFileName, records, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation(
                "{Source}: extraccion y staging completados en {ElapsedMs} ms. Registros: {Count}",
                extractor.SourceName,
                stopwatch.ElapsedMilliseconds,
                records.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extrayendo {Source}: {Message}", extractor.SourceName, ex.Message);
        }
    }
}
