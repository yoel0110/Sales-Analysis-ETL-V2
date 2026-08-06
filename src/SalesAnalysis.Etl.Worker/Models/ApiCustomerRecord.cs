namespace SalesAnalysis.Etl.Worker.Models;

public sealed class ApiCustomerRecord
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
}
