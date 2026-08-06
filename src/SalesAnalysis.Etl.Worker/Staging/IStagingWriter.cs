namespace SalesAnalysis.Etl.Worker.Staging;

public interface IStagingWriter
{
    Task WriteAsync<T>(string fileName, IReadOnlyCollection<T> records, CancellationToken cancellationToken = default);
}
