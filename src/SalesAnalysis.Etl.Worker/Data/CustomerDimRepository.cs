using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Etl.Worker.Data.Entities;

namespace SalesAnalysis.Etl.Worker.Data;

public sealed class CustomerDimRepository : ICustomerDimRepository
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<CustomerDimRepository> _logger;

    public CustomerDimRepository(WarehouseDbContext context, ILogger<CustomerDimRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task TruncateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Vaciando CustomerDim");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM CustomerDim;", cancellationToken).ConfigureAwait(false);
    }

    public async Task BulkInsertAsync(IReadOnlyCollection<CustomerDim> entities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Insertando {Count} registros en CustomerDim", entities.Count);
        await _context.CustomerDims.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("CustomerDim cargada correctamente");
    }

    public async Task<Dictionary<int, int>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo lookup de CustomerDim");
        var lookup = await _context.CustomerDims
            .AsNoTracking()
            .ToDictionaryAsync(c => c.CustomerId, c => c.CustomerDimId, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Lookup de CustomerDim obtenido: {Count} registros", lookup.Count);
        return lookup;
    }
}
