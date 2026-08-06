using CsvHelper.Configuration;
using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker.Extractors.Mappings;

public sealed class CsvOrderRecordMap : ClassMap<CsvOrderRecord>
{
    public CsvOrderRecordMap()
    {
        Map(m => m.OrderId).Name("OrderID");
        Map(m => m.CustomerId).Name("CustomerID");
        Map(m => m.OrderDate).Name("OrderDate").TypeConverterOption.Format("yyyy-MM-dd");
        Map(m => m.Status).Name("Status");
    }
}
