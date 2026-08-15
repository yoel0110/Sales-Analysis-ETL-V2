using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker.Data;

public sealed class FactTableRepository : IFactTableRepository
{
    private readonly WarehouseDbContext _context;
    private readonly ILogger<FactTableRepository> _logger;

    public FactTableRepository(WarehouseDbContext context, ILogger<FactTableRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task TruncateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Vaciando FactTable");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM FactTable;", cancellationToken).ConfigureAwait(false);
    }

    public async Task BulkInsertAsync(IReadOnlyCollection<FactTableLoadRecord> records, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enviando {Count} registros a usp_InsertFactTable", records.Count);

        var factTableTvp = new DataTable();
        factTableTvp.Columns.Add("OrderId", typeof(int));
        factTableTvp.Columns.Add("CustomerId", typeof(int));
        factTableTvp.Columns.Add("ProductId", typeof(int));
        factTableTvp.Columns.Add("OrderDate", typeof(DateTime));
        factTableTvp.Columns.Add("Quantity", typeof(int));
        factTableTvp.Columns.Add("TotalPrice", typeof(decimal));

        foreach (var record in records)
        {
            factTableTvp.Rows.Add(
                record.OrderId,
                record.CustomerId,
                record.ProductId,
                record.OrderDate.Date,
                record.Quantity,
                record.TotalPrice);
        }

        var connectionString = _context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("No se encontro la cadena de conexion de olap_ventas.");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = new SqlCommand("dbo.usp_InsertFactTable", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@FactTableTVP", SqlDbType.Structured)
        {
            TypeName = "dbo.TVP_FactTable",
            Value = factTableTvp
        });

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("FactTable cargada correctamente");
    }
}
