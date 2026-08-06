using CsvHelper.Configuration;
using SalesAnalysis.Etl.Worker.Models;

namespace SalesAnalysis.Etl.Worker.Extractors.Mappings;

public sealed class CsvProductRecordMap : ClassMap<CsvProductRecord>
{
    public CsvProductRecordMap()
    {
        Map(m => m.ProductId).Name("ProductID");
        Map(m => m.ProductName).Name("ProductName");
        Map(m => m.Category).Name("Category");
        Map(m => m.Price).Name("Price");
        Map(m => m.Stock).Name("Stock");
    }
}
