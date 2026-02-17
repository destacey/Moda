using System.ComponentModel.DataAnnotations;

namespace Moda.Common.Domain.Enums.AppIntegrations;

// Max length of 32 characters
public enum Connector
{
    [Display(Name = "Azure DevOps", Description = "The Azure DevOps connector enables Moda to connect and retrieve data for the following areas and synchronize it:  Projects, Areas, Iterations, Work Items, Work Item Types.  This synchronization is one-way.")]
    AzureDevOps = 0,

    [Display(Name = "Azure OpenAI", Description = "The Azure OpenAI connector enables Moda to connect and retrieve data from Azure-hosted OpenAI services for AI-powered features.")]
    AzureOpenAI = 1,

    [Display(Name = "OpenAI", Description = "The OpenAI connector enables Moda to connect to OpenAI API for LLM capabilities.")]
    OpenAI = 2
}
