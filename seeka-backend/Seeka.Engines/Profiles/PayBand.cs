namespace Seeka.Engines.Profiles;

/// <summary>A monthly pay range; the top band has no maximum.</summary>
public sealed record PayBand(decimal Minimum, decimal? Maximum);
