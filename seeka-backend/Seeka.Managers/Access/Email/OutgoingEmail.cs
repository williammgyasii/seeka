using Seeka.Engines.Accounts;

namespace Seeka.Managers.Access.Email;

public sealed record OutgoingEmail(string To, EmailContent Content);
