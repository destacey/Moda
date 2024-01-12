using Moda.Common.Domain.Enums.Goals;
using Moda.Goals.Domain.Enums;

namespace Moda.Goals.Domain.Tests.Data;
public class ObjectiveFaker : Faker<Objective>
{
    public ObjectiveFaker(ObjectiveType type, ObjectiveStatus status, Guid? ownerId, Guid? planId, LocalDate? start, LocalDate? end)
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int());
        RuleFor(x => x.Name, f => f.Random.String2(10));
        RuleFor(x => x.Description, f => f.Random.String2(10));
        RuleFor(x => x.Type, type);
        RuleFor(x => x.Status, status);
        RuleFor(x => x.Progress, f => SetProgress(status, f));
        RuleFor(x => x.OwnerId, ownerId);
        RuleFor(x => x.PlanId, planId);
        RuleFor(x => x.StartDate, start);
        RuleFor(x => x.TargetDate, end);
    }

    private static double SetProgress(ObjectiveStatus status, Faker faker)
    {
        var progress = faker.Random.Double(0, 99);
        return status switch
        {
            ObjectiveStatus.NotStarted => 0.0d,
            ObjectiveStatus.InProgress => progress,
            ObjectiveStatus.Completed => 100.0d,
            ObjectiveStatus.Canceled => progress,
            ObjectiveStatus.Missed => progress,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
