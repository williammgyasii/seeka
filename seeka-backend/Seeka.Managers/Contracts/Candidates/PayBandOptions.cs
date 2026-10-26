using Seeka.Engines.Profiles;

namespace Seeka.Managers.Contracts.Candidates;

/// <param name="ByCountry">Each country's default pay currency, keyed by ISO 3166 code.</param>
public sealed record PayBandOptions(
    IReadOnlyDictionary<string, IReadOnlyList<PayBand>> ByCurrency,
    IReadOnlyDictionary<string, string> ByCountry);
