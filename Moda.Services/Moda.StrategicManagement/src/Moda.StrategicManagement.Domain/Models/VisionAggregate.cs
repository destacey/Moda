using CSharpFunctionalExtensions;
using Moda.StrategicManagement.Domain.Enums;
using NodaTime;

namespace Moda.StrategicManagement.Domain.Models;
public sealed class VisionAggregate(List<Vision> visions)
{
    private readonly List<Vision> _visions = visions;

    public IReadOnlyList<Vision> Visions => _visions.AsReadOnly();

    /// <summary>
    /// Activate a proposed vision and set the start date.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="activationDate"></param>
    /// <returns></returns>
    public Result Activate(Guid id, Instant activationDate)
    {
        var vision = _visions.SingleOrDefault(v => v.Id == id);
        if (vision is null)
        {
            return Result.Failure("Vision not found.");
        }

        if (_visions.Any(v => v.State == VisionState.Active))
        {
            return Result.Failure("An active vision already exists. Only one active vision is allowed.");
        }

        var mostRecentArchivedVision = _visions
            .Where(v => v.State == VisionState.Archived)
            .OrderByDescending(v => v.Dates!.Start)
            .FirstOrDefault();
        if (mostRecentArchivedVision is not null && mostRecentArchivedVision.Dates!.Includes(activationDate))
        {
            return Result.Failure("The vision cannot be activated because it overlaps with another archived vision.");
        }

        return vision.Activate(activationDate);
    }

    /// <summary>
    /// Archive an active vision and set the end date.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="archiveDate"></param>
    /// <returns></returns>
    public Result Archive(Guid id, Instant archiveDate)
    {
        var vision = _visions.SingleOrDefault(v => v.Id == id);
        if (vision is null)
        {
            return Result.Failure("Vision not found.");
        }

        var canArchiveResult = vision.CanArchive(archiveDate);
        if (canArchiveResult.IsFailure)
        {
            return canArchiveResult;
        }

        var archivedVisions = _visions
            .Where(v => v.State == VisionState.Archived)
            .OrderByDescending(v => v.Dates!.Start)
            .ToList();

        if (archivedVisions.Count != 0 && vision.Dates!.Start < archivedVisions.First().Dates!.End)
        {
            var visionDates = new FlexibleInstantRange(vision.Dates!.Start, archiveDate);
            foreach (var archive in archivedVisions)
            {
                if (archive.Dates!.Overlaps(visionDates))
                {
                    return Result.Failure("The vision cannot be archived because it overlaps with another archived vision.");
                }
            }
        }

        return vision.Archive(archiveDate);
    }
}
