using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Seeka.Application;
using Seeka.Infrastructure;
using Seeka.Infrastructure.Entities;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Seeka")
    ?? throw new InvalidOperationException("ConnectionStrings:Seeka is required.");

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<SeekaExceptionHandler>();
builder.Services.AddDbContext<SeekaDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDataProtection()
    .SetApplicationName("Seeka")
    .PersistKeysToDbContext<SeekaDbContext>();
builder.Services.Configure<ForwardedHeadersOptions>(
    options =>
    {
        var configured = SeekaRuntimePolicy.CreateForwardedHeadersOptions();
        options.ForwardedHeaders = configured.ForwardedHeaders;
        options.ForwardLimit = configured.ForwardLimit;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 10;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<SeekaDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
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
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
if (SeekaRuntimePolicy.ShouldRunHangfireServer(
        builder.Environment.EnvironmentName,
        builder.Configuration))
{
    builder.Services.AddHangfireServer();
}
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<WorkspaceContext>();
builder.Services.AddScoped<FirstSliceManager>();

var app = builder.Build();
if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<SeekaDbContext>();
    await db.Database.MigrateAsync();
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
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => TypedResults.Ok(new { status = "healthy" }));
app.MapGet(SeekaApiRoutes.Health, () => TypedResults.Ok(new { status = "healthy" }));
var api = app.MapGroup("/api");

api.MapPost("/auth/register", async (
    RegisterRequest request,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    SeekaDbContext db,
    TimeProvider timeProvider,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.DisplayName))
        return Results.BadRequest(new ErrorResponse("Display name is required."));
    var now = timeProvider.GetUtcNow();
    await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
    var user = new AppUser
    {
        Id = Guid.NewGuid(),
        UserName = request.Email.Trim(),
        Email = request.Email.Trim(),
        DisplayName = request.DisplayName.Trim(),
        CreatedAt = now
    };
    var creation = await userManager.CreateAsync(user, request.Password);
    if (!creation.Succeeded)
        return Results.ValidationProblem(creation.Errors.ToDictionary(
            error => error.Code,
            error => new[] { error.Description }));
    var workspace = new Workspace
    {
        Id = Guid.NewGuid(),
        Name = $"{user.DisplayName}'s workspace",
        PersonalOwnerUserId = user.Id,
        CreatedAt = now
    };
    db.Workspaces.Add(workspace);
    db.WorkspaceMemberships.Add(new WorkspaceMembership
    {
        Id = Guid.NewGuid(),
        WorkspaceId = workspace.Id,
        UserId = user.Id,
        Role = WorkspaceRole.Owner,
        CreatedAt = now
    });
    await db.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);
    await signInManager.SignInAsync(user, isPersistent: true);
    return Results.Ok(new SessionResponse(
        user.Id,
        user.Email!,
        user.DisplayName,
        workspace.Id));
}).RequireRateLimiting("auth");

api.MapPost("/auth/login", async (
    LoginRequest request,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    SeekaDbContext db,
    CancellationToken cancellationToken) =>
{
    var user = await userManager.FindByEmailAsync(request.Email.Trim());
    if (user is null) return Results.Unauthorized();
    var result = await signInManager.PasswordSignInAsync(
        user,
        request.Password,
        isPersistent: true,
        lockoutOnFailure: true);
    if (!result.Succeeded) return Results.Unauthorized();
    var workspaceId = await db.WorkspaceMemberships
        .Where(item => item.UserId == user.Id)
        .OrderBy(item => item.CreatedAt)
        .Select(item => item.WorkspaceId)
        .FirstAsync(cancellationToken);
    return Results.Ok(new SessionResponse(
        user.Id,
        user.Email!,
        user.DisplayName,
        workspaceId));
}).RequireRateLimiting("auth");

api.MapPost("/auth/logout", async (SignInManager<AppUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.NoContent();
}).RequireAuthorization();

api.MapGet("/auth/session", async (
    HttpContext context,
    WorkspaceContext workspaceContext,
    UserManager<AppUser> userManager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    if (workspace is null) return Results.Unauthorized();
    var user = await userManager.FindByIdAsync(workspace.UserId.ToString());
    return user is null
        ? Results.Unauthorized()
        : Results.Ok(new SessionResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            workspace.WorkspaceId));
}).RequireAuthorization();

var workspaceApi = api.MapGroup("")
    .RequireAuthorization();

workspaceApi.MapGet("/candidate", async (
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    if (workspace is null) return Results.Unauthorized();
    var setup = await manager.GetSetupAsync(workspace.WorkspaceId, cancellationToken);
    return setup is null ? Results.NotFound() : Results.Ok(setup);
});

workspaceApi.MapPut("/candidate", async (
    CandidateSetupRequest request,
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    return workspace is null
        ? Results.Unauthorized()
        : Results.Ok(await manager.SaveSetupAsync(
            workspace.WorkspaceId,
            request,
            cancellationToken));
});

workspaceApi.MapPost("/jobs/import", async (
    ImportJobRequest request,
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    return workspace is null
        ? Results.Unauthorized()
        : Results.Created(
            $"/api/jobs",
            await manager.ImportJobAsync(
                workspace.WorkspaceId,
                workspace.UserId,
                request,
                cancellationToken));
});

workspaceApi.MapGet("/pipeline", async (
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    return workspace is null
        ? Results.Unauthorized()
        : Results.Ok(await manager.GetPipelineAsync(
            workspace.WorkspaceId,
            cancellationToken));
});

workspaceApi.MapGet("/jobs/{savedJobId:guid}", async (
    Guid savedJobId,
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    if (workspace is null) return Results.Unauthorized();
    var detail = await manager.GetJobDetailAsync(
        workspace.WorkspaceId,
        savedJobId,
        cancellationToken);
    return detail is null ? Results.NotFound() : Results.Ok(detail);
});

workspaceApi.MapPut("/jobs/{savedJobId:guid}/stage/{stage}", async (
    Guid savedJobId,
    PipelineStage stage,
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    if (workspace is null) return Results.Unauthorized();
    return await manager.UpdateStageAsync(
        workspace.WorkspaceId,
        workspace.UserId,
        savedJobId,
        stage,
        cancellationToken)
        ? Results.NoContent()
        : Results.NotFound();
});

workspaceApi.MapPost("/jobs/{savedJobId:guid}/contacts", async (
    Guid savedJobId,
    AddContactRequest request,
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    if (workspace is null) return Results.Unauthorized();
    var contact = await manager.AddContactAsync(
        workspace.WorkspaceId,
        workspace.UserId,
        savedJobId,
        request,
        cancellationToken);
    return contact is null ? Results.NotFound() : Results.Created($"/api/jobs/{savedJobId}", contact);
});

workspaceApi.MapPost("/jobs/{savedJobId:guid}/drafts/{contactId:guid}", async (
    Guid savedJobId,
    Guid contactId,
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    if (workspace is null) return Results.Unauthorized();
    var drafts = await manager.GenerateDraftsAsync(
        workspace.WorkspaceId,
        workspace.UserId,
        savedJobId,
        contactId,
        cancellationToken);
    return drafts is null ? Results.NotFound() : Results.Ok(drafts);
});

workspaceApi.MapPut("/drafts/{draftId:guid}", async (
    Guid draftId,
    UpdateDraftRequest request,
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    if (workspace is null) return Results.Unauthorized();
    return await manager.UpdateDraftAsync(
        workspace.WorkspaceId,
        draftId,
        request,
        cancellationToken)
        ? Results.NoContent()
        : Results.NotFound();
});

workspaceApi.MapPost("/jobs/{savedJobId:guid}/activities", async (
    Guid savedJobId,
    RecordActivityRequest request,
    HttpContext context,
    WorkspaceContext workspaceContext,
    FirstSliceManager manager,
    CancellationToken cancellationToken) =>
{
    var workspace = await workspaceContext.ResolveAsync(context.User, cancellationToken);
    if (workspace is null) return Results.Unauthorized();
    return await manager.RecordActivityAsync(
        workspace.WorkspaceId,
        workspace.UserId,
        savedJobId,
        request,
        cancellationToken)
        ? Results.Created($"/api/jobs/{savedJobId}", null)
        : Results.NotFound();
});

app.Run();

public partial class Program;
