﻿using System.ComponentModel.DataAnnotations;

namespace Moda.AppIntegration.Domain.Enums;
public enum Connector
{
    [Display(Name = "Azure DevOps Boards", Description = "The Azure DevOps Boards connector enables Moda to connect and retrieve data for the following areas and sychronize the it:  Projects, Areas, Iterations, Work Items, Work Item Types.  This sychronization is one-way.")]
    AzureDevOpsBoards = 0
}
