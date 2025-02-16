using Moda.Common.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Data;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;

public sealed class ProgramFaker : PrivateConstructorFaker<Program>
{
    public ProgramFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Commerce.Department());
        RuleFor(x => x.Description, f => f.Lorem.Paragraph());
        RuleFor(x => x.Status, f => ProgramStatus.Proposed);
        RuleFor(x => x.DateRange, f => null); // Default is null, as proposed programs may not have dates.
        RuleFor(x => x.PortfolioId, f => f.Random.Guid()); // Set by portfolio in real scenarios.
    }
}

public static class ProgramFakerExtensions
{
    public static ProgramFaker WithData(
        this ProgramFaker faker,
        Guid? id = null,
        string? name = null,
        string? description = null,
        ProgramStatus? status = null,
        LocalDateRange? dateRange = null,
        Guid? portfolioId = null,
        Dictionary<ProgramRole, HashSet<Guid>>? roles = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (status.HasValue) { faker.RuleFor(x => x.Status, status); }
        if (dateRange is not null) { faker.RuleFor(x => x.DateRange, dateRange); }
        if (portfolioId.HasValue) { faker.RuleFor(x => x.PortfolioId, portfolioId.Value); }

        if (roles is not null)
        {
            var programId = id ?? Guid.NewGuid();
            if (!id.HasValue)
            {
                faker.RuleFor(x => x.Id, programId);
            }

            HashSet<RoleAssignment<ProgramRole>> updatedRoles = new();
            foreach (var role in roles)
            {
                foreach (var employeeId in role.Value)
                {
                    updatedRoles.Add(new RoleAssignment<ProgramRole>(programId, role.Key, employeeId));
                }
            }

            faker.RuleFor("_roles", x => updatedRoles);
        }

        return faker;
    }

    /// <summary>
    /// Generates an active program with a start date 10 days ago.
    /// </summary>
    public static Program ActiveProgram(this ProgramFaker faker, TestingDateTimeProvider dateTimeProvider, Guid? portfolioId = null)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-10);
        var endDate = startDate.PlusDays(10);

        return faker.WithData(
            status: ProgramStatus.Active,
            dateRange: new LocalDateRange(startDate, endDate),
            portfolioId: portfolioId
        ).Generate();
    }

    /// <summary>
    /// Generates a completed program with a start date 20 days ago and end date 10 days ago.
    /// </summary>
    public static Program CompletedProgram(this ProgramFaker faker, TestingDateTimeProvider dateTimeProvider, Guid? portfolioId = null)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-20);
        var endDate = startDate.PlusDays(5);

        return faker.WithData(
            status: ProgramStatus.Completed,
            dateRange: new LocalDateRange(startDate, endDate),
            portfolioId: portfolioId
        ).Generate();
    }

    /// <summary>
    /// Generates a proposed program without a date range.
    /// </summary>
    public static Program ProposedProgram(this ProgramFaker faker, Guid? portfolioId = null)
    {
        return faker.WithData(
            status: ProgramStatus.Proposed,
            portfolioId: portfolioId
        ).Generate();
    }

    /// <summary>
    /// Generates a cancelled program with a start date 15 days ago and an end date 5 days ago.
    /// </summary>
    public static Program CancelledProgram(this ProgramFaker faker, TestingDateTimeProvider dateTimeProvider, Guid? portfolioId = null)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-15);
        var endDate = startDate.PlusDays(5);

        return faker.WithData(
            status: ProgramStatus.Cancelled,
            dateRange: new LocalDateRange(startDate, endDate),
            portfolioId: portfolioId
        ).Generate();
    }
}
