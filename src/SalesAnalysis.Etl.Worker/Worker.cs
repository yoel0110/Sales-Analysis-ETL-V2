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
    private readonly IFactTableRepository _factTableRepository;
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
        IFactTableRepository factTableRepository,
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
        _factTableRepository = factTableRepository;
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

        await _factTableRepository.TruncateAsync(stoppingToken).ConfigureAwait(false);

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
        var productPriceLookup = products.ToDictionary(p => p.ProductId, p => p.Price);
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

        var orderDetails = await _csvOrderDetailExtractor.ExtractAsync(stoppingToken).ConfigureAwait(false);
        var orderLookup = orders.ToDictionary(o => o.OrderId);
        var customerDimLookup = await _customerDimRepository.GetLookupAsync(stoppingToken).ConfigureAwait(false);
        var productDimLookup = await _productDimRepository.GetLookupAsync(stoppingToken).ConfigureAwait(false);
        var dateDimLookup = await _dateDimRepository.GetLookupAsync(stoppingToken).ConfigureAwait(false);

        var factEntities = new List<FactTable>();
        int inconsistencyCount = 0;
        int skippedCount = 0;

        foreach (var detail in orderDetails)
        {
            if (!orderLookup.TryGetValue(detail.OrderId, out var order))
            {
                _logger.LogWarning("OrderID {OrderId} no encontrado. Se omite detalle.", detail.OrderId);
                skippedCount++;
                continue;
            }

            if (!customerDimLookup.TryGetValue(order.CustomerId, out var customerDimId))
            {
                _logger.LogWarning("CustomerID {CustomerId} no encontrado en CustomerDim. Se omite detalle.", order.CustomerId);
                skippedCount++;
                continue;
            }

            if (!productDimLookup.TryGetValue(detail.ProductId, out var productDimId))
            {
                _logger.LogWarning("ProductID {ProductId} no encontrado en ProductDim. Se omite detalle.", detail.ProductId);
                skippedCount++;
                continue;
            }

            var orderDate = order.OrderDate.Date;
            if (!dateDimLookup.TryGetValue(orderDate, out var dateDimId))
            {
                _logger.LogWarning("Fecha {OrderDate} no encontrada en DateDim. Se omite detalle.", orderDate);
                skippedCount++;
                continue;
            }

            if (!productPriceLookup.TryGetValue(detail.ProductId, out var unitPrice))
            {
                _logger.LogWarning("ProductID {ProductId} no encontrado en productos para recalcular precio. Se omite detalle.", detail.ProductId);
                skippedCount++;
                continue;
            }

            var calculatedTotalPrice = detail.Quantity * unitPrice;
            var finalTotalPrice = detail.TotalPrice;

            if (calculatedTotalPrice != finalTotalPrice)
            {
                _logger.LogWarning(
                    "Inconsistencia detectada: OrderID={OrderId}, ProductID={ProductId}, Original={Original}, Recalculado={Recalculado}. Se usa el valor recalculado.",
                    detail.OrderId,
                    detail.ProductId,
                    finalTotalPrice,
                    calculatedTotalPrice);
                finalTotalPrice = calculatedTotalPrice;
                inconsistencyCount++;
            }

            factEntities.Add(new FactTable
            {
                OrderId = detail.OrderId,
                CustomerDimId = customerDimId,
                ProductDimId = productDimId,
                DateDimId = dateDimId,
                Quantity = detail.Quantity,
                TotalPrice = finalTotalPrice
            });
        }

        await _factTableRepository.BulkInsertAsync(factEntities, stoppingToken).ConfigureAwait(false);

        _logger.LogInformation(
            "FactTable cargada con {Count} registros. Inconsistencias corregidas: {Inconsistencies}. Omitidos: {Skipped}.",
            factEntities.Count,
            inconsistencyCount,
            skippedCount);
    }
}
