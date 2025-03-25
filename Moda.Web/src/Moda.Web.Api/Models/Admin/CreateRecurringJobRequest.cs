namespace Moda.Web.Api.Models.Admin;

public class CreateRecurringJobRequest
{
    public string JobId { get; set; } = default!;
    public int JobTypeId { get; set; }
    public string CronExpression { get; set; } = default!;
}
