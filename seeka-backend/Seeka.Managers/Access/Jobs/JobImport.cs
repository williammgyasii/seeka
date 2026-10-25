using Seeka.Engines.Eligibility;
using Seeka.Engines.Matching;
using Seeka.Managers.Access.Pipeline;

namespace Seeka.Managers.Access.Jobs;

public sealed record JobImport(
    Guid WorkspaceId,
    NewJobPosting Posting,
    EligibilityResult Eligibility,
    string OriginalRestrictionText,
    MatchResult Match,
    ActivityEntry Activity,
    DateTimeOffset ImportedAt);
