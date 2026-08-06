namespace SalesAnalysis.Etl.Worker.Options;

public sealed class ApiSourceOptions
{
    public string CustomersUrl { get; set; } = "http://localhost:5000/api/customers";
    public string ProductsUrl { get; set; } = "http://localhost:5000/api/products";
    public int TimeoutSeconds { get; set; } = 30;
}
