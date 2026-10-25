using Seeka.Engines.Matching;

namespace Seeka.Managers.Access.Jobs;

public sealed record ExistingJob(string CompanyName, JobSnapshot Snapshot);
