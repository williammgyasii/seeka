namespace Seeka.Api.Http;

public static class AuthPolicies
{
    /// <summary>
    /// Only checks the session. For the few endpoints a person needs before their
    /// email is verified; everything else uses the verified default policy.
    /// </summary>
    public const string SignedIn = "signed-in";
}
