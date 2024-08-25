using Moda.Common.Application.Interfaces.Work;
using Moda.Common.Application.Validators;

namespace Moda.Common.Application.Requests.WorkManagement;

/// <summary>
/// 
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="WorkItems"></param>
/// <param name="teamMappings">The key is set to the external team id and value is set to the internal (Moda) team id.</param>
public sealed record SyncExternalWorkItemsCommand(Guid WorkspaceId, List<IExternalWorkItem> WorkItems, Dictionary<Guid, Guid?> teamMappings) : ICommand;

public sealed class SyncExternalWorkItemsCommandValidator : CustomValidator<SyncExternalWorkItemsCommand>
{
    public SyncExternalWorkItemsCommandValidator()
    {
        RuleFor(c => c.WorkspaceId)
            .NotEmpty();

        RuleForEach(c => c.WorkItems)
            .NotNull()
            .SetValidator(new IExternalWorkItemValidator());

        RuleFor(c => c.teamMappings)
            .NotNull();

        When(c => c.teamMappings.Count > 0, () =>
        {
            RuleForEach(c => c.teamMappings).ChildRules(teamMapping =>
            {
                teamMapping.RuleFor(tm => tm.Key)
                    .NotEmpty();

                teamMapping.When(tm => tm.Value.HasValue, () =>
                {
                    teamMapping.RuleFor(tm => tm.Value)
                        .NotEmpty()
                        .Must(v => v.HasValue && v.Value != Guid.Empty);
                });
            });
        });
    }
}
