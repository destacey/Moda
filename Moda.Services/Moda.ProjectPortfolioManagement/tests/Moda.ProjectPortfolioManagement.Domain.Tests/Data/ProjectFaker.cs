using Moda.Common.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Data;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;

public sealed class ProjectFaker : PrivateConstructorFaker<Project>
{
    public ProjectFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int(1000, 10000));
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
        RuleFor(x => x.Description, f => f.Lorem.Paragraph());
        RuleFor(x => x.Status, f => ProjectStatus.Proposed);
        RuleFor(x => x.DateRange, f => null); // Default is null for proposed projects.
        RuleFor(p => p.ExpenditureCategoryId, f => f.Random.Int(1, 10));
        RuleFor(x => x.PortfolioId, f => f.Random.Guid()); // Set by portfolio in real scenarios.
        RuleFor(x => x.ProgramId, f => null); // Optional, can be null by default.
    }
}

public static class ProjectFakerExtensions
{
    public static ProjectFaker WithData(
        this ProjectFaker faker,
        Guid? id = null,
        int? key = null,
        string? name = null,
        string? description = null,
        ProjectStatus? status = null,
        LocalDateRange? dateRange = null,
        int? expenditureCategoryId = null,
        Guid? portfolioId = null,
        Guid? programId = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (key.HasValue) { faker.RuleFor(x => x.Key, key.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (status.HasValue) { faker.RuleFor(x => x.Status, status); }
        if (dateRange is not null) { faker.RuleFor(x => x.DateRange, dateRange); }
        if (expenditureCategoryId.HasValue) faker.RuleFor(p => p.ExpenditureCategoryId, expenditureCategoryId.Value);
        if (portfolioId.HasValue) { faker.RuleFor(x => x.PortfolioId, portfolioId.Value); }
        if (programId.HasValue) { faker.RuleFor(x => x.ProgramId, programId.Value); }

        return faker;
    }

    /// <summary>
    /// Generates an active project with a start date 10 days ago.
    /// </summary>
    public static Project ActiveProject(this ProjectFaker faker, TestingDateTimeProvider dateTimeProvider, Guid? portfolioId = null, Guid? programId = null)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-10);
        var endDate = startDate.PlusMonths(5);

        return faker.WithData(
            status: ProjectStatus.Active,
            dateRange: new LocalDateRange(startDate, endDate),
            portfolioId: portfolioId,
            programId: programId
        ).Generate();
    }

    /// <summary>
    /// Generates a completed project with a start date 20 days ago and an end date 10 days ago.
    /// </summary>
    public static Project CompletedProject(this ProjectFaker faker, TestingDateTimeProvider dateTimeProvider, Guid? portfolioId = null, Guid? programId = null)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-20);
        var endDate = now.PlusDays(-10);

        return faker.WithData(
            status: ProjectStatus.Completed,
            dateRange: new LocalDateRange(startDate, endDate),
            portfolioId: portfolioId,
            programId: programId
        ).Generate();
    }

    /// <summary>
    /// Generates a proposed project without a date range.
    /// </summary>
    public static Project ProposedProject(this ProjectFaker faker, Guid? portfolioId = null, Guid? programId = null)
    {
        return faker.WithData(
            status: ProjectStatus.Proposed,
            portfolioId: portfolioId,
            programId: programId
        ).Generate();
    }

    /// <summary>
    /// Generates a cancelled project with a start date 15 days ago and an end date 5 days ago.
    /// </summary>
    public static Project CancelledProject(this ProjectFaker faker, TestingDateTimeProvider dateTimeProvider, Guid? portfolioId = null, Guid? programId = null)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-15);
        var endDate = now.PlusDays(-5);

        return faker.WithData(
            status: ProjectStatus.Cancelled,
            dateRange: new LocalDateRange(startDate, endDate),
            portfolioId: portfolioId,
            programId: programId
        ).Generate();
    }
}
