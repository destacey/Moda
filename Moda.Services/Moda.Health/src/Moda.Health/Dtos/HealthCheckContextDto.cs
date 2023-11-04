﻿namespace Moda.Health.Dtos;

public sealed record HealthCheckContextDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
