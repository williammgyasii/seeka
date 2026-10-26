using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Seeka.Api.Http;
using Seeka.Managers;
using Seeka.Managers.Membership;
using Seeka.ResourceAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Services.AddOpenApi();
builder.Services
    .AddControllers(options =>
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<SeekaExceptionHandler>();
builder.Services.Configure<ForwardedHeadersOptions>(
    options =>
    {
        var configured = HttpEdgePolicy.CreateForwardedHeadersOptions();
        options.ForwardedHeaders = configured.ForwardedHeaders;
        options.ForwardLimit = configured.ForwardLimit;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "__Host-seeka-session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
});
var emailVerified = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .AddRequirements(new EmailVerifiedRequirement())
    .Build();
builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(emailVerified)
    .SetFallbackPolicy(emailVerified)
    .AddPolicy(AuthPolicies.SignedIn, policy => policy.RequireAuthenticatedUser());
builder.Services.AddScoped<IAuthorizationHandler, EmailVerifiedHandler>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = ApiRateLimits.CreateGlobal();
    options.AddPolicy(ApiRateLimits.Auth, ApiRateLimits.AuthPartition);
    options.AddPolicy(ApiRateLimits.Resume, ApiRateLimits.ResumePartition);
});
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(new AppLinks(builder.Configuration["App:PublicUrl"] ?? "http://localhost:5173"));
builder.Services.AddSeekaResourceAccess(builder.Configuration, builder.Environment);
builder.Services.AddSeekaManagers();

var app = builder.Build();
if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    await app.Services.MigrateSeekaDatabaseAsync();
    return;
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    var fetchSite = context.Request.Headers["Sec-Fetch-Site"].ToString();
    if (!SameOriginRequestPolicy.IsAllowed(context.Request.Method, fetchSite))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }

    await next(context);
});
if (app.Environment.IsDevelopment()) app.MapOpenApi().AllowAnonymous();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
