namespace SalesAnalysis.Etl.Worker.Data.Entities;

public sealed class DateDim
{
    public int DateDimId { get; set; }
    public DateTime Fecha { get; set; }
    public int Day { get; set; }
    public string DayName { get; set; } = string.Empty;
    public bool IsWeekend { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string Quarters { get; set; } = string.Empty;
    public int Year { get; set; }
}
