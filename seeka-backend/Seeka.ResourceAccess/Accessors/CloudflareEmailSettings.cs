namespace Seeka.ResourceAccess.Accessors;

internal sealed record CloudflareEmailSettings(
    string AccountId,
    string ApiToken,
    string FromAddress,
    string FromName);
