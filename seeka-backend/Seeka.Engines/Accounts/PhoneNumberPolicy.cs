using PhoneNumbers;

namespace Seeka.Engines.Accounts;

public static class PhoneNumberPolicy
{
    private static readonly PhoneNumberUtil Numbers = PhoneNumberUtil.GetInstance();

    /// <summary>
    /// Returns the number in E.164 form when it is valid for its country, otherwise null.
    /// Input must carry its country calling code (a leading "+").
    /// </summary>
    public static string? ToE164(string input)
    {
        var trimmed = input.Trim();
        if (!trimmed.StartsWith('+')) return null;
        try
        {
            var number = Numbers.Parse(trimmed, null);
            return Numbers.IsValidNumber(number)
                ? Numbers.Format(number, PhoneNumberFormat.E164)
                : null;
        }
        catch (NumberParseException)
        {
            return null;
        }
    }
}
