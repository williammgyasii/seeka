namespace Seeka.Engines.Profiles;

/// <summary>Level is one of <see cref="ProfileLimits.LanguageLevels"/>, or unknown.</summary>
public sealed record LanguageSkill(string Language, string? Level);
