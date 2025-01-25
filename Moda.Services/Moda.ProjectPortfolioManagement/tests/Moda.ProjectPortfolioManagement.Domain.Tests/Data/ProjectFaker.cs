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
        RuleFor(x => x.PortfolioId, f => f.Random.Guid());
        RuleFor(x => x.DateRange, f => null); // Default is null, as proposed projects may not have dates.
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
        Guid? portfolioId = null,
        FlexibleDateRange? dateRange = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (key.HasValue) { faker.RuleFor(x => x.Key, key.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (status.HasValue) { faker.RuleFor(x => x.Status, status); }
        if (portfolioId.HasValue) { faker.RuleFor(x => x.PortfolioId, portfolioId.Value); }
        if (dateRange is not null) { faker.RuleFor(x => x.DateRange, dateRange); }

        return faker;
    }

    /// <summary>
    /// Generates an active project with a start date 10 days ago.
    /// </summary>
    public static Project ActiveProject(this ProjectFaker faker, TestingDateTimeProvider dateTimeProvider)
    {
        var now = dateTimeProvider.Today;
        var defaultStartDate = now.PlusDays(-10);

        return faker.WithData(
            status: ProjectStatus.Active,
            dateRange: new FlexibleDateRange(defaultStartDate)
        ).Generate();
    }

    /// <summary>
    /// Generates a completed project with a start date 20 days ago and end date 10 days ago.
    /// </summary>
    public static Project CompletedProject(this ProjectFaker faker, TestingDateTimeProvider dateTimeProvider)
    {
        var now = dateTimeProvider.Today;
        var defaultStartDate = now.PlusDays(-20);
        var defaultEndDate = now.PlusDays(-10);

        return faker.WithData(
            status: ProjectStatus.Completed,
            dateRange: new FlexibleDateRange(defaultStartDate, defaultEndDate)
        ).Generate();
    }

    /// <summary>
    /// Generates a cancelled project with a start date and end date.
    /// </summary>
    public static Project CancelledProject(this ProjectFaker faker, TestingDateTimeProvider dateTimeProvider)
    {
        var now = dateTimeProvider.Today;
        var defaultStartDate = now.PlusDays(-30);
        var defaultEndDate = now.PlusDays(-20);

        return faker.WithData(
            status: ProjectStatus.Cancelled,
            dateRange: new FlexibleDateRange(defaultStartDate, defaultEndDate)
        ).Generate();
    }
}

