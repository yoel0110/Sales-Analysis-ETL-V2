using Microsoft.Data.SqlClient;
using SalesAnalysis.Api.Models;

namespace SalesAnalysis.Api.Data;

public sealed class SalesRepository : ISalesRepository, IAsyncDisposable
{
    private readonly SqlConnection _connection;

    public SalesRepository(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("connection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("La cadena de conexion 'connection' no esta configurada.");
        }

        _connection = new SqlConnection(connectionString);
    }

    public async Task<IReadOnlyCollection<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT
                c.CustomerID AS CustomerId,
                c.FirstName,
                c.LastName,
                c.Email,
                c.Phone,
                co.CountryName,
                ci.CityName
            FROM Customers c
            INNER JOIN Countries co ON c.CountryID = co.CountryID
            INNER JOIN Cities ci ON c.CityID = ci.CityID;
            """;

        await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

        var customers = new List<CustomerResponse>();
        using var command = new SqlCommand(query, _connection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            customers.Add(new CustomerResponse
            {
                CustomerId = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                CountryName = reader.GetString(5),
                CityName = reader.GetString(6)
            });
        }

        return customers;
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT
                p.ProductID AS ProductId,
                p.ProductName,
                c.CategoryName,
                p.Price,
                p.Stock
            FROM Products p
            INNER JOIN Categories c ON p.CategoryID = c.CategoryID;
            """;

        await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

        var products = new List<ProductResponse>();
        using var command = new SqlCommand(query, _connection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            products.Add(new ProductResponse
            {
                ProductId = reader.GetInt32(0),
                ProductName = reader.GetString(1),
                CategoryName = reader.GetString(2),
                Price = reader.GetDecimal(3),
                Stock = reader.GetInt32(4)
            });
        }

        return products;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
