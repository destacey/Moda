using Moda.Organization.Domain.Enums;
using Moda.Organization.Domain.Models;

namespace Moda.Organization.Domain.Interfaces;
public interface ITeam
{
    Guid Id { get; }
    int LocalId { get; }
    string Name { get; }
    TeamCode Code { get; }
    string? Description { get; }
    TeamType Type { get; }
    bool IsActive { get; }
}