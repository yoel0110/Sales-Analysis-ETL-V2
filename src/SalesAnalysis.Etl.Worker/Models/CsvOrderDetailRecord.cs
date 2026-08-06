namespace SalesAnalysis.Etl.Worker.Models;

public sealed class CsvOrderDetailRecord
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
