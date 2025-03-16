using Moda.Common.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Data;
using NodaTime;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;

public class StrategicInitiativeFaker : PrivateConstructorFaker<StrategicInitiative>
{
    public StrategicInitiativeFaker(TestingDateTimeProvider dateTimeProvider)
    {
        var start = dateTimeProvider.Now.Plus(Duration.FromDays(FakerHub.Random.Int(1, 20))).InUtc().LocalDateTime.Date;
        var end = start.PlusDays(FakerHub.Random.Int(1, 20));

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int(1000, 10000));
        RuleFor(x => x.Name, f => f.Commerce.ProductName());
        RuleFor(x => x.Description, f => f.Lorem.Paragraph());
        RuleFor(x => x.Status, f => StrategicInitiativeStatus.Proposed);
        RuleFor(x => x.DateRange, f => new LocalDateRange(start, end)); 
        RuleFor(x => x.PortfolioId, f => f.Random.Guid());
    }
}

public static class StrategicInitiativeFakerExtensions
{
    public static StrategicInitiativeFaker WithData(
        this StrategicInitiativeFaker faker,
        Guid? id = null,
        int? key = null,
        string? name = null,
        string? description = null,
        StrategicInitiativeStatus? status = null,
        LocalDateRange? dateRange = null,
        Guid? portfolioId = null,
        Dictionary<StrategicInitiativeRole, HashSet<Guid>>? roles = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (key.HasValue) { faker.RuleFor(x => x.Key, key.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (status.HasValue) { faker.RuleFor(x => x.Status, status); }
        if (dateRange is not null) { faker.RuleFor(x => x.DateRange, dateRange); }
        if (portfolioId.HasValue) { faker.RuleFor(x => x.PortfolioId, portfolioId.Value); }

        if (roles is not null)
        {
            var initiativeId = id ?? Guid.NewGuid();
            if (!id.HasValue)
            {
                faker.RuleFor(x => x.Id, initiativeId);
            }
            HashSet<RoleAssignment<StrategicInitiativeRole>> updatedRoles = new();
            foreach (var role in roles)
            {
                var roleId = Guid.NewGuid();
                foreach (var userId in role.Value)
                {
                    updatedRoles.Add(new RoleAssignment<StrategicInitiativeRole>(initiativeId, role.Key, userId));
                }
            }
            faker.RuleFor(x => x.Roles, updatedRoles);
        }
        return faker;
    }

    /// <summary>
    /// Creates a strategic initiative with the status of Active.
    /// </summary>
    /// <param name="faker"></param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="portfolioId"></param>
    /// <returns></returns>
    public static StrategicInitiative Active(
        this StrategicInitiativeFaker faker,
        TestingDateTimeProvider dateTimeProvider, 
        Guid? portfolioId = null)
    { 
        var start = dateTimeProvider.Now.Plus(Duration.FromDays(-10)).InUtc().LocalDateTime.Date;
        var end = start.PlusDays(200);

        return faker.WithData(
            status: StrategicInitiativeStatus.Active,
            dateRange: new LocalDateRange(start, end),
            portfolioId: portfolioId
        ).Generate();
    }

    /// <summary>
    /// Creates a strategic initiative with the status of Completed.
    /// </summary>
    /// <param name="faker"></param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="portfolioId"></param>
    /// <returns></returns>
    public static StrategicInitiative Completed(
        this StrategicInitiativeFaker faker,
        TestingDateTimeProvider dateTimeProvider,
        Guid? portfolioId = null)
    {
        var start = dateTimeProvider.Now.Plus(Duration.FromDays(-200)).InUtc().LocalDateTime.Date;
        var end = start.PlusDays(100);

        return faker.WithData(
            status: StrategicInitiativeStatus.Completed,
            dateRange: new LocalDateRange(start, end),
            portfolioId: portfolioId
        ).Generate();
    }

    /// <summary>
    /// Creates a strategic initiative with the status of Cancelled.
    /// </summary>
    /// <param name="faker"></param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="portfolioId"></param>
    /// <returns></returns>
    public static StrategicInitiative Cancelled(
        this StrategicInitiativeFaker faker,
        TestingDateTimeProvider dateTimeProvider,
        Guid? portfolioId = null)
    {
        var start = dateTimeProvider.Now.Plus(Duration.FromDays(-200)).InUtc().LocalDateTime.Date;
        var end = start.PlusDays(100);

        return faker.WithData(
            status: StrategicInitiativeStatus.Cancelled,
            dateRange: new LocalDateRange(start, end),
            portfolioId: portfolioId
        ).Generate();
    }
}
