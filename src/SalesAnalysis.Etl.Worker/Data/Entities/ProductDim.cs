namespace SalesAnalysis.Etl.Worker.Data.Entities;

public sealed class ProductDim
{
    public int ProductDimId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
