namespace Moda.Common.Application.Requests.WorkManagement.Interfaces;

public interface IWorkTypeDto
{
    string? Description { get; set; }
    int Id { get; set; }
    bool IsActive { get; set; }
    string Name { get; set; }
}
