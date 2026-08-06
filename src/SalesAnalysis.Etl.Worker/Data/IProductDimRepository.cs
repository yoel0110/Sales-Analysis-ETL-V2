using SalesAnalysis.Etl.Worker.Data.Entities;

namespace SalesAnalysis.Etl.Worker.Data;

public interface IProductDimRepository
{
    Task TruncateAsync(CancellationToken cancellationToken = default);
    Task BulkInsertAsync(IReadOnlyCollection<ProductDim> entities, CancellationToken cancellationToken = default);
    Task<Dictionary<int, int>> GetLookupAsync(CancellationToken cancellationToken = default);
}
