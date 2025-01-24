using Moda.Common.Models;
using Moda.StrategicManagement.Domain.Enums;
using Moda.StrategicManagement.Domain.Models;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Data;
using NodaTime;
using NodaTime.Extensions;

namespace Moda.StrategicManagement.Domain.Tests.Data;
public sealed class VisionFaker : PrivateConstructorFaker<Vision>
{
    public VisionFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int(1000, 10000));
        RuleFor(x => x.Description, f => f.Lorem.Sentence(15));
        RuleFor(x => x.State, f => VisionState.Proposed);
        RuleFor(x => x.Dates, f => null);
    }
}

public static class VisionFakerExtensions
{
    public static VisionFaker WithData(this VisionFaker faker, Guid? id = null, int? key = null, string? description = null, VisionState? state = null, FlexibleInstantRange? dates = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (key.HasValue) { faker.RuleFor(x => x.Key, key.Value); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (state.HasValue) { faker.RuleFor(x => x.State, state); }
        if (dates is not null) { faker.RuleFor(x => x.Dates, dates); }

        return faker;
    }

    /// <summary>
    /// Generates an active Vision that started 10 days ago.
    /// </summary>
    /// <param name="faker"></param>
    /// <param name="dateTimeProvider"></param>
    /// <returns></returns>
    public static Vision ActiveVision(this VisionFaker faker, TestingDateTimeProvider? dateTimeProvider = null)
    {
        var now = dateTimeProvider?.Now ?? DateTime.UtcNow.ToInstant();
        var defaultActiveInstant = now.Plus(Duration.FromDays(-10));

        return faker.WithData(
            state: VisionState.Active, 
            dates: new FlexibleInstantRange(defaultActiveInstant)
            ).Generate();
    }

    /// <summary>
    /// Generates an archived Vision that started 20 days ago and ended 10 days ago.
    /// </summary>
    /// <param name="faker"></param>
    /// <param name="dateTimeProvider"></param>
    /// <returns></returns>
    public static Vision ArchivedVision(this VisionFaker faker, TestingDateTimeProvider? dateTimeProvider = null)
    {
        var now = dateTimeProvider?.Now ?? DateTime.UtcNow.ToInstant();
        var defaultActiveInstant = now.Plus(Duration.FromDays(-20));
        var defaultArchivedInstant = defaultActiveInstant.Plus(Duration.FromDays(10));

        return faker.WithData(
            state: VisionState.Archived,
            dates: new FlexibleInstantRange(defaultActiveInstant, defaultArchivedInstant)
            ).Generate();
    }
}
