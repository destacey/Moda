﻿using Mapster;

namespace Moda.Links.Models;
public sealed record LinkDto : IMapFrom<Link>
{
    public Guid Id { get; set; }
    public Guid ObjectId { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
}
