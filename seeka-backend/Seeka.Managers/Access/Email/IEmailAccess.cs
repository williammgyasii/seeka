namespace Seeka.Managers.Access.Email;

public interface IEmailAccess
{
    /// <summary>Returns false when the provider did not accept the email; never throws for provider failures.</summary>
    Task<bool> SendAsync(OutgoingEmail email, CancellationToken cancellationToken);
}
