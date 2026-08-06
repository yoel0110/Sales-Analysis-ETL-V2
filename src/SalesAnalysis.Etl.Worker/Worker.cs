using SalesAnalysis.Etl.Worker.Data;
using SalesAnalysis.Etl.Worker.Data.Entities;
using SalesAnalysis.Etl.Worker.Extractors;
using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ICustomerDimRepository _customerDimRepository;
    private readonly IExtractor<CsvCustomerRecord> _csvCustomerExtractor;
    private readonly IExtractor<CsvProductRecord> _csvProductExtractor;
    private readonly IExtractor<CsvOrderRecord> _csvOrderExtractor;
    private readonly IExtractor<CsvOrderDetailRecord> _csvOrderDetailExtractor;
    private readonly IExtractor<ApiCustomerRecord> _apiCustomerExtractor;
    private readonly IExtractor<ApiProductRecord> _apiProductExtractor;

    public Worker(
        ILogger<Worker> logger,
        ICustomerDimRepository customerDimRepository,
        IExtractor<CsvCustomerRecord> csvCustomerExtractor,
        IExtractor<CsvProductRecord> csvProductExtractor,
        IExtractor<CsvOrderRecord> csvOrderExtractor,
        IExtractor<CsvOrderDetailRecord> csvOrderDetailExtractor,
        IExtractor<ApiCustomerRecord> apiCustomerExtractor,
        IExtractor<ApiProductRecord> apiProductExtractor)
    {
        _logger = logger;
        _customerDimRepository = customerDimRepository;
        _csvCustomerExtractor = csvCustomerExtractor;
        _csvProductExtractor = csvProductExtractor;
        _csvOrderExtractor = csvOrderExtractor;
        _csvOrderDetailExtractor = csvOrderDetailExtractor;
        _apiCustomerExtractor = apiCustomerExtractor;
        _apiProductExtractor = apiProductExtractor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando carga al Data Warehouse");

        var customers = await _csvCustomerExtractor.ExtractAsync(stoppingToken).ConfigureAwait(false);
        var customerEntities = customers.Select(c => new CustomerDim
        {
            CustomerId = c.CustomerId,
            FullName = $"{c.FirstName} {c.LastName}",
            CountryName = c.Country,
            CityName = c.City
        }).ToList();

        await _customerDimRepository.TruncateAsync(stoppingToken).ConfigureAwait(false);
        await _customerDimRepository.BulkInsertAsync(customerEntities, stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("CustomerDim cargada con {Count} registros", customerEntities.Count);
    }
}
