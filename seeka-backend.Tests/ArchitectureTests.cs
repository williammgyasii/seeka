using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Seeka.Engines.Matching;
using Seeka.Managers.JobSearch;
using Seeka.Managers;
using Seeka.ResourceAccess;

namespace Seeka.Backend.Tests;

public sealed partial class ArchitectureTests
{
    private static readonly Assembly Engines = typeof(MatchEngine).Assembly;
    private static readonly Assembly Managers = typeof(JobSearchManager).Assembly;
    private static readonly Assembly ResourceAccess =
        typeof(global::Seeka.ResourceAccess.DependencyInjection).Assembly;
    private static readonly Assembly Client = typeof(Program).Assembly;

    private static readonly string[] Frameworks =
        ["Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "Npgsql", "Hangfire"];

    [Fact]
    public void Engines_reference_no_framework_and_no_other_layer() =>
        AssertNoReferences(Engines, [.. Frameworks, "Seeka."]);

    [Fact]
    public void Managers_reference_only_engines() =>
        AssertNoReferences(Managers, [.. Frameworks, "Seeka.ResourceAccess", "Seeka.Api"]);

    [Fact]
    public void Resource_access_never_references_the_client() =>
        AssertNoReferences(ResourceAccess, ["Seeka.Api"]);

    [Fact]
    public void Controllers_depend_only_on_managers()
    {
        var controllers = Client.GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
            .ToArray();
        Assert.NotEmpty(controllers);

        var injected = controllers.SelectMany(controller =>
            controller.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters())
                .Concat(controller
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .SelectMany(action => action.GetParameters())
                    .Where(parameter => parameter.GetCustomAttribute<FromServicesAttribute>() is not null))
                .Select(parameter => (controller, parameter.ParameterType)));

        AssertNone(
            injected.Where(item => !IsManager(item.ParameterType)),
            item => $"{item.controller.Name} injects {item.ParameterType.Name}");
    }

    [Fact]
    public void Managers_never_call_other_managers()
    {
        var managers = Managers.GetTypes().Where(IsManager).ToArray();
        Assert.Equal(3, managers.Length);

        AssertNone(
            managers.SelectMany(manager => ConstructorDependencies(manager)
                .Where(IsManager)
                .Select(dependency => (manager, dependency))),
            item => $"{item.manager.Name} depends on {item.dependency.Name}");
    }

    [Fact]
    public void Accessors_never_call_other_accessors()
    {
        var accessors = ResourceAccess.GetTypes()
            .Where(type => type.IsClass && type.GetInterfaces().Any(IsAccessContract))
            .ToArray();
        Assert.NotEmpty(accessors);

        AssertNone(
            accessors.SelectMany(accessor => ConstructorDependencies(accessor)
                .Where(dependency => IsAccessContract(dependency) || accessors.Contains(dependency))
                .Select(dependency => (accessor, dependency))),
            item => $"{item.accessor.Name} depends on {item.dependency.Name}");
    }

    [Fact]
    public void Client_maps_no_minimal_api_endpoints() =>
        AssertNone(
            BackendSourceFiles()
                .Where(path => path.Contains($"{Path.DirectorySeparatorChar}Seeka.Api{Path.DirectorySeparatorChar}"))
                .Where(path => MinimalEndpoint().IsMatch(File.ReadAllText(path))),
            path => Path.GetFileName(path));

    [Fact]
    public void Every_source_file_declares_one_top_level_type() =>
        AssertNone(
            BackendSourceFiles()
                .Select(path => (path, count: TopLevelType().Matches(File.ReadAllText(path)).Count))
                .Where(item => item.count > 1),
            item => $"{Path.GetFileName(item.path)} declares {item.count} types");

    private static bool IsManager(Type type) =>
        type.Assembly == Managers && type.IsClass && type.Name.EndsWith("Manager", StringComparison.Ordinal);

    private static bool IsAccessContract(Type type) =>
        type.IsInterface &&
        type.Assembly == Managers &&
        (type.Namespace ?? "").StartsWith("Seeka.Managers.Access", StringComparison.Ordinal);

    private static IEnumerable<Type> ConstructorDependencies(Type type) =>
        type.GetConstructors().SelectMany(constructor => constructor.GetParameters())
            .Select(parameter => parameter.ParameterType);

    private static void AssertNoReferences(Assembly assembly, string[] forbiddenPrefixes) =>
        AssertNone(
            assembly.GetReferencedAssemblies()
                .Select(reference => reference.Name ?? "")
                .Where(name => forbiddenPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal))),
            name => $"{assembly.GetName().Name} references {name}");

    private static void AssertNone<T>(IEnumerable<T> violations, Func<T, string> describe)
    {
        var found = violations.Select(describe).ToArray();
        Assert.True(found.Length == 0, string.Join(Environment.NewLine, found));
    }

    private static IEnumerable<string> BackendSourceFiles()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Seeka.slnx")))
            directory = directory.Parent;
        Assert.NotNull(directory);

        var separator = Path.DirectorySeparatorChar;
        return Directory
            .EnumerateFiles(Path.Combine(directory.FullName, "seeka-backend"), "*.cs", SearchOption.AllDirectories)
            .Where(path =>
                !path.Contains($"{separator}bin{separator}") &&
                !path.Contains($"{separator}obj{separator}") &&
                !path.Contains($"{separator}Migrations{separator}"));
    }

    [GeneratedRegex(
        @"^(?:(?:public|internal|file|sealed|static|abstract|partial|readonly)\s+)*(?:class|record|interface|enum|struct)\s+\w+",
        RegexOptions.Multiline)]
    private static partial Regex TopLevelType();

    [GeneratedRegex(@"\.Map(?:Get|Post|Put|Delete|Patch|Group)\(")]
    private static partial Regex MinimalEndpoint();
}
