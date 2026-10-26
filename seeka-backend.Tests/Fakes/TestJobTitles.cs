using Seeka.Engines.Profiles;

namespace Seeka.Backend.Tests.Fakes;

/// <summary>A few made-up ESCO-style occupations; the real list is too large for tests.</summary>
internal static class TestJobTitles
{
    public static readonly JobTitleCatalog Catalog = new(
    [
        new Occupation("human resources manager", ["HR manager", "personnel manager"]),
        new Occupation("human resources officer", ["HR officer", "people partner"]),
        new Occupation("software developer", ["programmer", "software engineer"]),
        new Occupation("payroll clerk", [])
    ]);
}
