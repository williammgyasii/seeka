using Microsoft.AspNetCore.Identity;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Domain { get; set; }
}
