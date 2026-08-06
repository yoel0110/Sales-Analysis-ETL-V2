using SalesAnalysis.Etl.Worker.Data.Entities;

namespace SalesAnalysis.Etl.Worker.Data;

public interface IFactTableRepository
{
    Task TruncateAsync(CancellationToken cancellationToken = default);
    Task BulkInsertAsync(IReadOnlyCollection<FactTable> entities, CancellationToken cancellationToken = default);
}
