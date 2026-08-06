namespace SalesAnalysis.Etl.Worker.Data.Entities;

public sealed class FactTable
{
    public long FactId { get; set; }
    public int OrderId { get; set; }
    public int CustomerDimId { get; set; }
    public int ProductDimId { get; set; }
    public int DateDimId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
