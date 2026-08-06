using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Etl.Worker.Data.Entities;

namespace SalesAnalysis.Etl.Worker.Data;

public sealed class FactTableRepository : IFactTableRepository
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<FactTableRepository> _logger;

    public FactTableRepository(WarehouseDbContext context, ILogger<FactTableRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task TruncateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Truncando FactTable");
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE FactTable;", cancellationToken).ConfigureAwait(false);
    }

    public async Task BulkInsertAsync(IReadOnlyCollection<FactTable> entities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Insertando {Count} registros en FactTable", entities.Count);
        await _context.Facts.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("FactTable cargada correctamente");
    }
}
