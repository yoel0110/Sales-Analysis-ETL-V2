using System.Globalization;
using SalesAnalysis.Etl.Worker.Data;
using SalesAnalysis.Etl.Worker.Data.Entities;
using SalesAnalysis.Etl.Worker.Extractors;
using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var csvCustomerExtractor = scope.ServiceProvider.GetRequiredService<IExtractor<CsvCustomerRecord>>();
        var csvProductExtractor = scope.ServiceProvider.GetRequiredService<IExtractor<CsvProductRecord>>();
        var csvOrderExtractor = scope.ServiceProvider.GetRequiredService<IExtractor<CsvOrderRecord>>();
        var csvOrderDetailExtractor = scope.ServiceProvider.GetRequiredService<IExtractor<CsvOrderDetailRecord>>();
        var customerDimRepository = scope.ServiceProvider.GetRequiredService<ICustomerDimRepository>();
        var productDimRepository = scope.ServiceProvider.GetRequiredService<IProductDimRepository>();
        var dateDimRepository = scope.ServiceProvider.GetRequiredService<IDateDimRepository>();
        var factTableRepository = scope.ServiceProvider.GetRequiredService<IFactTableRepository>();

        _logger.LogInformation("Iniciando carga al Data Warehouse");

        await factTableRepository.TruncateAsync(stoppingToken).ConfigureAwait(false);

        var customers = await csvCustomerExtractor.ExtractAsync(stoppingToken).ConfigureAwait(false);
        var customerEntities = customers.Select(c => new CustomerDim
        {
            CustomerId = c.CustomerId,
            FullName = $"{c.FirstName} {c.LastName}",
            CountryName = c.Country,
            CityName = c.City
        }).ToList();

        await customerDimRepository.TruncateAsync(stoppingToken).ConfigureAwait(false);
        await customerDimRepository.BulkInsertAsync(customerEntities, stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("CustomerDim cargada con {Count} registros", customerEntities.Count);

        var products = await csvProductExtractor.ExtractAsync(stoppingToken).ConfigureAwait(false);
        var productPriceLookup = products.ToDictionary(p => p.ProductId, p => p.Price);
        var productEntities = products.Select(p => new ProductDim
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            CategoryName = p.Category,
            Price = p.Price
        }).ToList();

        await productDimRepository.TruncateAsync(stoppingToken).ConfigureAwait(false);
        await productDimRepository.BulkInsertAsync(productEntities, stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("ProductDim cargada con {Count} registros", productEntities.Count);

        var orders = await csvOrderExtractor.ExtractAsync(stoppingToken).ConfigureAwait(false);
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

        await dateDimRepository.TruncateAsync(stoppingToken).ConfigureAwait(false);
        await dateDimRepository.BulkInsertAsync(dateEntities, stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("DateDim cargada con {Count} registros", dateEntities.Count);

        var orderDetails = await csvOrderDetailExtractor.ExtractAsync(stoppingToken).ConfigureAwait(false);
        var orderLookup = orders.ToDictionary(o => o.OrderId);
        var customerDimLookup = await customerDimRepository.GetLookupAsync(stoppingToken).ConfigureAwait(false);
        var productDimLookup = await productDimRepository.GetLookupAsync(stoppingToken).ConfigureAwait(false);
        var dateDimLookup = await dateDimRepository.GetLookupAsync(stoppingToken).ConfigureAwait(false);

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

        await factTableRepository.BulkInsertAsync(factEntities, stoppingToken).ConfigureAwait(false);

        _logger.LogInformation(
            "FactTable cargada con {Count} registros. Inconsistencias corregidas: {Inconsistencies}. Omitidos: {Skipped}.",
            factEntities.Count,
            inconsistencyCount,
            skippedCount);
    }
}
