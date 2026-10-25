using Seeka.Managers.Contracts.Outreach;
using Seeka.Managers.Contracts.Pipeline;

namespace Seeka.Managers.Contracts.Jobs;

public sealed record JobDetailResponse(
    SavedJobView SavedJob,
    JobPostingView Job,
    EligibilityView? Eligibility,
    MatchView? Match,
    IReadOnlyList<ContactView> Contacts,
    IReadOnlyList<OutreachDraftView> Drafts,
    IReadOnlyList<ActivityView> Activities);
