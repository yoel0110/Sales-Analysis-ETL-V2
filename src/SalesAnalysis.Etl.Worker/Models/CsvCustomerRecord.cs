namespace SalesAnalysis.Etl.Worker.Models;

public sealed class CsvCustomerRecord
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
