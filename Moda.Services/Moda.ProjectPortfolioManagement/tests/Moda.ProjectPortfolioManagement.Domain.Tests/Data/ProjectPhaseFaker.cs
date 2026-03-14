using Moda.Common.Models;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared.Data;
using TaskStatus = Moda.ProjectPortfolioManagement.Domain.Enums.TaskStatus;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;

public sealed class ProjectPhaseFaker : PrivateConstructorFaker<ProjectPhase>
{
    public ProjectPhaseFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.ProjectId, f => f.Random.Guid());
        RuleFor(x => x.ProjectLifecyclePhaseId, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
        RuleFor(x => x.Description, f => f.Lorem.Paragraph());
        RuleFor(x => x.Status, f => TaskStatus.NotStarted);
        RuleFor(x => x.Order, f => f.Random.Int(1, 10));
        RuleFor(x => x.Progress, f => Progress.NotStarted());
    }
}

public static class ProjectPhaseFakerExtensions
{
    public static ProjectPhaseFaker WithData(
        this ProjectPhaseFaker faker,
        Guid? id = null,
        Guid? projectId = null,
        Guid? projectLifecyclePhaseId = null,
        string? name = null,
        string? description = null,
        TaskStatus? status = null,
        int? order = null,
        Progress? progress = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (projectId.HasValue) { faker.RuleFor(x => x.ProjectId, projectId.Value); }
        if (projectLifecyclePhaseId.HasValue) { faker.RuleFor(x => x.ProjectLifecyclePhaseId, projectLifecyclePhaseId.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (status.HasValue) { faker.RuleFor(x => x.Status, status.Value); }
        if (order.HasValue) { faker.RuleFor(x => x.Order, order.Value); }
        if (progress is not null) { faker.RuleFor(x => x.Progress, progress); }

        return faker;
    }
}
