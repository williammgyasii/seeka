using Seeka.Managers.Access.Email;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeEmailAccess(bool succeeds = true) : IEmailAccess
{
    public List<OutgoingEmail> Sent { get; } = [];

    public Task<bool> SendAsync(OutgoingEmail email, CancellationToken cancellationToken)
    {
        Sent.Add(email);
        return Task.FromResult(succeeds);
    }
}
