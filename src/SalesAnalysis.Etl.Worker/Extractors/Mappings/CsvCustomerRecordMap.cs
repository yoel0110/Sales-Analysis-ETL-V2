using CsvHelper.Configuration;
using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker.Extractors.Mappings;

public sealed class CsvCustomerRecordMap : ClassMap<CsvCustomerRecord>
{
    public CsvCustomerRecordMap()
    {
        Map(m => m.CustomerId).Name("CustomerID");
        Map(m => m.FirstName).Name("FirstName");
        Map(m => m.LastName).Name("LastName");
        Map(m => m.Email).Name("Email").Optional();
        Map(m => m.Phone).Name("Phone").Optional();
        Map(m => m.City).Name("City");
        Map(m => m.Country).Name("Country");
    }
}
