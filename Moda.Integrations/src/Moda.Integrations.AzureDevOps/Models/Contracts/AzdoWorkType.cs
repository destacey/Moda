﻿using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoWorkType : IExternalWorkTypeWorkflow
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string BacklogLevelId { get; set; }
    public bool IsActive { get; set; }
    public IList<IExternalWorkflowState> WorkflowStates { get; set; } = [];
}
