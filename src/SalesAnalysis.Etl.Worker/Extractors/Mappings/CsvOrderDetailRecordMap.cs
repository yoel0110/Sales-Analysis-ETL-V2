using CsvHelper.Configuration;
using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker.Extractors.Mappings;

public sealed class CsvOrderDetailRecordMap : ClassMap<CsvOrderDetailRecord>
{
    public CsvOrderDetailRecordMap()
    {
        Map(m => m.OrderId).Name("OrderID");
        Map(m => m.ProductId).Name("ProductID");
        Map(m => m.Quantity).Name("Quantity");
        Map(m => m.TotalPrice).Name("TotalPrice");
    }
}
