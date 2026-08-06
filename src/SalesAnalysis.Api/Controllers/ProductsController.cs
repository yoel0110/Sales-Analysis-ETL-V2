using Microsoft.AspNetCore.Mvc;
using SalesAnalysis.Api.Data;
using SalesAnalysis.Api.Models;

namespace SalesAnalysis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ISalesRepository _repository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ISalesRepository repository, ILogger<ProductsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> Get(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando consulta de productos");
        var products = await _repository.GetProductsAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Productos obtenidos: {Count}", products.Count);
        return Ok(products);
    }
}
