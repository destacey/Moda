namespace Moda.Common.Application.BackgroundJobs;
public sealed record BackgroundJobTypeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public required string GroupName { get; set; }
}
