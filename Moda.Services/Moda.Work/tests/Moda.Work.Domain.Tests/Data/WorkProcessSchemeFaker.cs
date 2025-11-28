using Moda.Tests.Shared.Data;
using Moda.Tests.Shared.Extensions;
using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Tests.Data;

public class WorkProcessSchemeFaker : PrivateConstructorFaker<WorkProcessScheme>
{
    private readonly WorkType? _workType;
    private readonly Workflow? _workflow;

    public WorkProcessSchemeFaker()
    {
        _workType = new WorkTypeFaker().Generate();
        _workflow = new WorkflowFaker().Generate();

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.WorkProcessId, f => f.Random.Guid());
        RuleFor(x => x.IsActive, true);
        
        // Set navigation properties after construction using stored references
        FinishWith((f, scheme) =>
        {
            if (_workType != null)
            {
                typeof(WorkProcessScheme).GetProperty("WorkType")!.SetValue(scheme, _workType);
            }
            if (_workflow != null)
            {
                typeof(WorkProcessScheme).GetProperty("Workflow")!.SetValue(scheme, _workflow);
            }
        });
    }
}

public static class WorkProcessSchemeFakerExtensions
{
    public static WorkProcessSchemeFaker WithWorkType(this WorkProcessSchemeFaker faker, WorkType workType)
    {
        faker.SetPrivateField("_workType", workType);
        return faker;
    }

    public static WorkProcessSchemeFaker WithWorkflow(this WorkProcessSchemeFaker faker, Workflow workflow)
    {
        faker.SetPrivateField("_workflow", workflow);
        return faker;
    }

    public static WorkProcessSchemeFaker WithIsActive(this WorkProcessSchemeFaker faker, bool isActive)
    {
        faker.RuleFor(x => x.IsActive, isActive);
        return faker;
    }
}
