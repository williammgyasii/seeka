namespace Seeka.Engines.Profiles;

/// <summary>One ESCO occupation: the name we store, and the other names people search by.</summary>
public sealed record Occupation(string Title, string[] Alternatives);
