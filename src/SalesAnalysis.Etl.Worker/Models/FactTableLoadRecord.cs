namespace SalesAnalysis.Etl.Worker.Models;

public sealed class FactTableLoadRecord
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public DateTime OrderDate { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
