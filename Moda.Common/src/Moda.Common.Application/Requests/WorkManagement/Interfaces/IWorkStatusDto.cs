namespace Moda.Common.Application.Requests.WorkManagement.Interfaces;
public interface IWorkStatusDto
{
    int Id { get; set; }
    string Name { get; set; }
    string? Description { get; set; }
    bool IsActive { get; set; }
}
