namespace SalesAnalysis.Etl.Worker.Models;

public sealed class CsvOrderRecord
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
