using Seeka.Engines.Profiles;

namespace Seeka.Managers.Contracts.Candidates;

/// <summary>Either the saved profile or the field errors that stopped the save.</summary>
public sealed record ProfileSaveResult(
    CandidateProfileResponse? Profile,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static ProfileSaveResult Saved(CandidateProfileResponse profile) => new(profile, new Dictionary<string, string[]>());

    public static ProfileSaveResult Rejected(IReadOnlyDictionary<string, string[]> errors) => new(null, errors);
}
