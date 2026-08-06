using SalesAnalysis.Etl.Worker.Data.Entities;

namespace SalesAnalysis.Etl.Worker.Data;

public interface IDateDimRepository
{
    Task TruncateAsync(CancellationToken cancellationToken = default);
    Task BulkInsertAsync(IReadOnlyCollection<DateDim> entities, CancellationToken cancellationToken = default);
    Task<Dictionary<DateTime, int>> GetLookupAsync(CancellationToken cancellationToken = default);
}
