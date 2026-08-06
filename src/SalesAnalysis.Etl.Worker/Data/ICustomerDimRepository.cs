using SalesAnalysis.Etl.Worker.Data.Entities;

namespace SalesAnalysis.Etl.Worker.Data;

public interface ICustomerDimRepository
{
    Task TruncateAsync(CancellationToken cancellationToken = default);
    Task BulkInsertAsync(IReadOnlyCollection<CustomerDim> entities, CancellationToken cancellationToken = default);
    Task<Dictionary<int, int>> GetLookupAsync(CancellationToken cancellationToken = default);
}
