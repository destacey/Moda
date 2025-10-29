using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.AppIntegrations;

// Max length of 32 characters
public enum Connector
{
    [Display(Name = "Azure DevOps", Description = "The Azure DevOps connector enables Moda to connect and retrieve data for the following areas and sychronize the it:  Projects, Areas, Iterations, Work Items, Work Item Types.  This sychronization is one-way.")]
    AzureDevOps = 0
}
