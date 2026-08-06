using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Etl.Worker.Data.Entities;

namespace SalesAnalysis.Etl.Worker.Data;

public sealed class DateDimRepository : IDateDimRepository
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<DateDimRepository> _logger;

    public DateDimRepository(WarehouseDbContext context, ILogger<DateDimRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task TruncateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Truncando DateDim");
        await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE DateDim;", cancellationToken).ConfigureAwait(false);
    }

    public async Task BulkInsertAsync(IReadOnlyCollection<DateDim> entities, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Insertando {Count} registros en DateDim", entities.Count);
        await _context.DateDims.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("DateDim cargada correctamente");
    }

    public async Task<Dictionary<DateTime, int>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo lookup de DateDim");
        var lookup = await _context.DateDims
            .AsNoTracking()
            .ToDictionaryAsync(d => d.Fecha, d => d.DateDimId, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation("Lookup de DateDim obtenido: {Count} registros", lookup.Count);
        return lookup;
    }
}
