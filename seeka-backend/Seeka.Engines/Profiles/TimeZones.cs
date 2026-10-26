namespace Seeka.Engines.Profiles;

internal static class TimeZones
{
    /// <summary>
    /// True for an IANA name the runtime knows, such as Africa/Accra. Windows names
    /// like "Eastern Standard Time" also resolve on .NET, so they are ruled out.
    /// </summary>
    public static bool IsReal(string timeZone)
    {
        var id = timeZone.Trim();
        return (id.Contains('/') || id == "UTC") && TimeZoneInfo.TryFindSystemTimeZoneById(id, out _);
    }
}
