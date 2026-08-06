using SalesAnalysis.Api.Models;

namespace SalesAnalysis.Api.Data;

public interface ISalesRepository
{
    Task<IReadOnlyCollection<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductResponse>> GetProductsAsync(CancellationToken cancellationToken = default);
}
