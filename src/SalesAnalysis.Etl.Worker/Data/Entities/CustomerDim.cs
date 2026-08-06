namespace SalesAnalysis.Etl.Worker.Data.Entities;

public sealed class CustomerDim
{
    public int CustomerDimId { get; set; }
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
}
