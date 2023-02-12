namespace Moda.Infrastructure.GraphApi;

public class GraphApiSettings
{
    public bool Enabled { get; set; } = false;
    public string? BaseUrl { get; set; }
    public string? Scopes { get; set; }
    public string? AllEmployeesGroupObjectId { get; set; }
}
