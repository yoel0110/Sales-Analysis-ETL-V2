namespace SalesAnalysis.Api.Models;

public sealed class CustomerResponse
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
}
