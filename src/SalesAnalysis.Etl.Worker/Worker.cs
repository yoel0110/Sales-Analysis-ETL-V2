using System.Globalization;
using SalesAnalysis.Etl.Worker.Data;
using SalesAnalysis.Etl.Worker.Data.Entities;
using SalesAnalysis.Etl.Worker.Extractors;
using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ICustomerDimRepository _customerDimRepository;
    private readonly IProductDimRepository _productDimRepository;
    private readonly IDateDimRepository _dateDimRepository;
    private readonly IExtractor<CsvCustomerRecord> _csvCustomerExtractor;
    private readonly IExtractor<CsvProductRecord> _csvProductExtractor;
    private readonly IExtractor<CsvOrderRecord> _csvOrderExtractor;
    private readonly IExtractor<CsvOrderDetailRecord> _csvOrderDetailExtractor;
    private readonly IExtractor<ApiCustomerRecord> _apiCustomerExtractor;
    private readonly IExtractor<ApiProductRecord> _apiProductExtractor;

    public Worker(
        ILogger<Worker> logger,
        ICustomerDimRepository customerDimRepository,
        IProductDimRepository productDimRepository,
        IDateDimRepository dateDimRepository,
        IExtractor<CsvCustomerRecord> csvCustomerExtractor,
        IExtractor<CsvProductRecord> csvProductExtractor,
        IExtractor<CsvOrderRecord> csvOrderExtractor,
        IExtractor<CsvOrderDetailRecord> csvOrderDetailExtractor,
        IExtractor<ApiCustomerRecord> apiCustomerExtractor,
        IExtractor<ApiProductRecord> apiProductExtractor)
    {
        _logger = logger;
        _customerDimRepository = customerDimRepository;
        _productDimRepository = productDimRepository;
        _dateDimRepository = dateDimRepository;
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

        var products = await _csvProductExtractor.ExtractAsync(stoppingToken).ConfigureAwait(false);
        var productEntities = products.Select(p => new ProductDim
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            CategoryName = p.Category,
            Price = p.Price
        }).ToList();

        await _productDimRepository.TruncateAsync(stoppingToken).ConfigureAwait(false);
        await _productDimRepository.BulkInsertAsync(productEntities, stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("ProductDim cargada con {Count} registros", productEntities.Count);

        var orders = await _csvOrderExtractor.ExtractAsync(stoppingToken).ConfigureAwait(false);
        var spanishCulture = new CultureInfo("es-ES");
        var dateEntities = orders
            .Select(o => o.OrderDate.Date)
            .Distinct()
            .Select(date => new DateDim
            {
                DateDimId = int.Parse(date.ToString("yyyyMMdd")),
                Fecha = date,
                Day = date.Day,
                DayName = date.ToString("dddd", spanishCulture),
                IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                Month = date.Month,
                MonthName = date.ToString("MMMM", spanishCulture),
                Quarters = $"Q{(date.Month - 1) / 3 + 1}",
                Year = date.Year
            })
            .ToList();

        await _dateDimRepository.TruncateAsync(stoppingToken).ConfigureAwait(false);
        await _dateDimRepository.BulkInsertAsync(dateEntities, stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("DateDim cargada con {Count} registros", dateEntities.Count);
    }
}
