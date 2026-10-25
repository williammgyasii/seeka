namespace Seeka.ResourceAccess.Accessors;

public static class EmailDeliveryPolicy
{
    public static EmailDelivery Choose(string environmentName, string? apiToken)
    {
        if (!string.IsNullOrWhiteSpace(apiToken)) return EmailDelivery.Cloudflare;
        return string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase)
            ? EmailDelivery.Log
            : EmailDelivery.Unconfigured;
    }
}
