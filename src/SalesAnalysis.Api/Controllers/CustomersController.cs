using Microsoft.AspNetCore.Mvc;
using SalesAnalysis.Api.Data;
using SalesAnalysis.Api.Models;

namespace SalesAnalysis.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ISalesRepository _repository;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ISalesRepository repository, ILogger<CustomersController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CustomerResponse>>> Get(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando consulta de clientes");
        var customers = await _repository.GetCustomersAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Clientes obtenidos: {Count}", customers.Count);
        return Ok(customers);
    }
}
