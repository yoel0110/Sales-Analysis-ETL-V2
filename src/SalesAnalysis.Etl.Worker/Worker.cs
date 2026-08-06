using SalesAnalysis.Etl.Worker.Extractors;
using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IExtractor<CsvCustomerRecord> _csvCustomerExtractor;
    private readonly IExtractor<CsvProductRecord> _csvProductExtractor;
    private readonly IExtractor<CsvOrderRecord> _csvOrderExtractor;
    private readonly IExtractor<CsvOrderDetailRecord> _csvOrderDetailExtractor;
    private readonly IExtractor<ApiCustomerRecord> _apiCustomerExtractor;
    private readonly IExtractor<ApiProductRecord> _apiProductExtractor;

    public Worker(
        ILogger<Worker> logger,
        IExtractor<CsvCustomerRecord> csvCustomerExtractor,
        IExtractor<CsvProductRecord> csvProductExtractor,
        IExtractor<CsvOrderRecord> csvOrderExtractor,
        IExtractor<CsvOrderDetailRecord> csvOrderDetailExtractor,
        IExtractor<ApiCustomerRecord> apiCustomerExtractor,
        IExtractor<ApiProductRecord> apiProductExtractor)
    {
        _logger = logger;
        _csvCustomerExtractor = csvCustomerExtractor;
        _csvProductExtractor = csvProductExtractor;
        _csvOrderExtractor = csvOrderExtractor;
        _csvOrderDetailExtractor = csvOrderDetailExtractor;
        _apiCustomerExtractor = apiCustomerExtractor;
        _apiProductExtractor = apiProductExtractor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker iniciado. Se prepara la carga directa al Data Warehouse.");
        await Task.CompletedTask;
    }
}
