namespace Moda.Integrations.AzureDevOps.Models.Projects;
internal record TeamDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
