﻿using Moda.Goals.Domain.Enums;
using Moda.Planning.Application.ProgramIncrements.Dtos;
using NodaTime.Extensions;

namespace Moda.Web.Api.Models.Planning.ProgramIncrements;

public class ImportProgramIncrementObjectivesRequest
{
    public int ImportId { get; set; }
    public Guid ProgramIncrementId { get; set; }
    public Guid TeamId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int StatusId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public bool IsStretch { get; set; }
    public DateTime? ClosedDateUtc { get; set; }

    public ImportProgramIncrementObjectiveDto ToImportProgramIncrementObjectiveDto()
    {
        LocalDate? startDate = StartDate?.ToLocalDateTime().Date;
        LocalDate? targetDate = TargetDate?.ToLocalDateTime().Date; 
        Instant? closedDateUtc = ClosedDateUtc.HasValue ? Instant.FromDateTimeUtc(DateTime.SpecifyKind(ClosedDateUtc.Value, DateTimeKind.Utc)) : null;
        return new ImportProgramIncrementObjectiveDto(ImportId, ProgramIncrementId, TeamId, Name, Description, (ObjectiveStatus)StatusId, startDate, targetDate, IsStretch, closedDateUtc);
    }
}

public sealed class ImportProgramIncrementObjectivesRequestValidator : CustomValidator<ImportProgramIncrementObjectivesRequest>
{
    public ImportProgramIncrementObjectivesRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(o => o.TeamId)
            .NotEmpty()
            .WithMessage("A plan must be selected.");

        RuleFor(o => o.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(o => o.Description)
            .MaximumLength(1024);

        RuleFor(o => (ObjectiveStatus)o.StatusId)
            .IsInEnum()
            .WithMessage("A valid status must be selected.");

        When(o => o.StartDate.HasValue && o.TargetDate.HasValue, () =>
        {
            RuleFor(o => o.StartDate)
                .LessThan(o => o.TargetDate)
                .WithMessage("The start date must be before the target date.");
        });

        When(o => (ObjectiveStatus)o.StatusId is ObjectiveStatus.Completed or ObjectiveStatus.Canceled or ObjectiveStatus.Incomplete,
            () => RuleFor(o => o.ClosedDateUtc)
                .NotEmpty()
                    .WithMessage("The ClosedDateUtc can not be empty if the status is Completed, Canceled, or Incomplete."))
            .Otherwise(() => RuleFor(o => o.ClosedDateUtc)
                .Empty()
                    .WithMessage("The ClosedDateUtc must be empty if the status is not Completed, Canceled, or Incomplete."));
    }
}
