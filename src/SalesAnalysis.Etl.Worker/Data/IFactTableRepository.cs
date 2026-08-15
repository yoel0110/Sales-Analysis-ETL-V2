using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker.Data;

public interface IFactTableRepository
{
    Task TruncateAsync(CancellationToken cancellationToken = default);
    Task BulkInsertAsync(IReadOnlyCollection<FactTableLoadRecord> records, CancellationToken cancellationToken = default);
}
