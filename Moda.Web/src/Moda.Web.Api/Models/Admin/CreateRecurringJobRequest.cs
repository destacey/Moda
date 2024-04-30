namespace Moda.Web.Api.Models.Admin;

public class CreateRecurringJobRequest
{
    public required string JobId { get; set; }
    public int JobTypeId { get; set; }
    public required string CronExpression { get; set; }
}
