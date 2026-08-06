namespace SalesAnalysis.Etl.Worker.Extractors;

public interface IExtractor<T>
{
    string SourceName { get; }
    Task<IReadOnlyCollection<T>> ExtractAsync(CancellationToken cancellationToken = default);
}
