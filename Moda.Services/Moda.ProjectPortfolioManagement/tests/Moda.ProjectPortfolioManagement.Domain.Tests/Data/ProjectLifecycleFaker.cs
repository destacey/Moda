using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared.Data;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;

public sealed class ProjectLifecycleFaker : PrivateConstructorFaker<ProjectLifecycle>
{
    public ProjectLifecycleFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int(1, 10000));
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
        RuleFor(x => x.Description, f => f.Lorem.Paragraph());
        RuleFor(x => x.State, f => ProjectLifecycleState.Proposed);
    }
}

public static class ProjectLifecycleFakerExtensions
{
    public static ProjectLifecycleFaker WithData(
        this ProjectLifecycleFaker faker,
        Guid? id = null,
        int? key = null,
        string? name = null,
        string? description = null,
        ProjectLifecycleState? state = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (key.HasValue) { faker.RuleFor(x => x.Key, key.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (state.HasValue) { faker.RuleFor(x => x.State, state.Value); }

        return faker;
    }

    /// <summary>
    /// Generates a proposed lifecycle with the specified phases.
    /// </summary>
    public static ProjectLifecycle AsProposedWithPhases(this ProjectLifecycleFaker faker, params (string Name, string Description)[] phases)
    {
        var lifecycle = faker.Generate();
        foreach (var (name, description) in phases)
        {
            lifecycle.AddPhase(name, description);
        }
        return lifecycle;
    }

    /// <summary>
    /// Generates an active lifecycle with the specified phases.
    /// </summary>
    public static ProjectLifecycle AsActiveWithPhases(this ProjectLifecycleFaker faker, params (string Name, string Description)[] phases)
    {
        var lifecycle = faker.AsProposedWithPhases(phases);
        lifecycle.Activate();
        return lifecycle;
    }

    /// <summary>
    /// Adds phases to an existing lifecycle using the lifecycle's AddPhase method.
    /// </summary>
    public static ProjectLifecycle WithPhases(this ProjectLifecycle lifecycle, params (string Name, string Description)[] phases)
    {
        foreach (var (name, description) in phases)
        {
            lifecycle.AddPhase(name, description);
        }
        return lifecycle;
    }
}
