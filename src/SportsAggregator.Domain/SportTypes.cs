namespace SportsAggregator.Domain;

public static class SportTypes
{
    public const string Football = "football";
    public const string Basketball = "basketball";
    public const string IceHockey = "ice_hockey";

    public static bool IsValid(string? sportType)
    {
        if (string.IsNullOrWhiteSpace(sportType))
        {
            return false;
        }

        return sportType.Trim().ToLowerInvariant() switch
        {
            Football => true,
            Basketball => true,
            IceHockey => true,
            _ => false
        };
    }
}
