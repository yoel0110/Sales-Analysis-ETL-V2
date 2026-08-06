using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Etl.Worker.Data.Entities;

namespace SalesAnalysis.Etl.Worker.Data;

public sealed class ProductDimRepository : IProductDimRepository
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<ProductDimRepository> _logger;

    public ProductDimRepository(WarehouseDbContext context, ILogger<ProductDimRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task TruncateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Truncando ProductDim");
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE ProductDim;", cancellationToken).ConfigureAwait(false);
    }

    public async Task BulkInsertAsync(IReadOnlyCollection<ProductDim> entities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Insertando {Count} registros en ProductDim", entities.Count);
        await _context.ProductDims.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("ProductDim cargada correctamente");
    }

    public async Task<Dictionary<int, int>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo lookup de ProductDim");
        var lookup = await _context.ProductDims
            .AsNoTracking()
            .ToDictionaryAsync(p => p.ProductId, p => p.ProductDimId, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Lookup de ProductDim obtenido: {Count} registros", lookup.Count);
        return lookup;
    }
}
