namespace SportsAggregator.Ingestion;

public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    public int IngestionIntervalSeconds { get; set; } = 30;
}
