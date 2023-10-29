using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Employees;
using Moda.Planning.Domain.Enums;
using NodaTime;

namespace Moda.Planning.Domain.Models;
public class Risk : BaseAuditableEntity<Guid>
{
    private string _summary = default!;
    private string? _description;
    private string? _response;

    private Risk() { }

    private Risk(string summary, string? description, Guid? teamId, Instant reportedOn, Guid reportedById, RiskCategory category, RiskGrade impact, RiskGrade likelihood, Guid? assigneeId, LocalDate? followUpDate, string? response)
    {
        Summary = summary;
        Description = description;
        TeamId = teamId;
        ReportedOn = reportedOn;
        ReportedById = reportedById;
        Category = category;
        Impact = impact;
        Likelihood = likelihood;
        AssigneeId = assigneeId;
        FollowUpDate = followUpDate;
        Response = response;

        Status = RiskStatus.Open;
    }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; private set; }

    /// <summary>
    /// The summary of the Risk.
    /// </summary>
    public string Summary
    {
        get => _summary;
        private set => _summary = Guard.Against.NullOrWhiteSpace(value, nameof(Summary)).Trim();
    }

    /// <summary>
    /// The description of the Risk.
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    // TODO: switch TeamId to ObjectId and Context
    public Guid? TeamId { get; private set; }

    public PlanningTeam? Team { get; set; }

    public Instant ReportedOn { get; private set; }

    public Guid ReportedById { get; private set; }

    public Employee ReportedBy { get; private set; } = default!;

    public RiskStatus Status { get; private set; }

    public RiskCategory Category { get; private set; }

    public RiskGrade Impact { get; private set; }

    public RiskGrade Likelihood { get; private set; }

    public RiskGrade Exposure 
    { 
        get
        {
            int exposure = (int)Impact + (int)Likelihood;
            return exposure switch
            {
                < 4 => RiskGrade.Low,
                4 => RiskGrade.Medium,
                _ => RiskGrade.High,
            };
        }
    }

    public Guid? AssigneeId { get; private set; }

    public Employee? Assignee { get; private set; }

    public LocalDate? FollowUpDate { get; private set; }

    public Instant? ClosedDate { get; private set; }

    /// <summary>
    /// What has been done to help prevent the risk from occurring.
    /// </summary>
    public string? Response
    {
        get => _response;
        private set => _response = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// Update an existing risk.
    /// </summary>
    /// <param name="summary"></param>
    /// <param name="description"></param>
    /// <param name="status"></param>
    /// <param name="category"></param>
    /// <param name="impact"></param>
    /// <param name="likelihood"></param>
    /// <param name="assigneeId"></param>
    /// <param name="followUpDate"></param>
    /// <param name="response"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result Update(string summary, string? description, RiskStatus status, RiskCategory category, RiskGrade impact, RiskGrade likelihood, Guid? assigneeId, LocalDate? followUpDate, string? response, Instant timestamp)
    {
        try
        {
            //TeamId isn't updatable at this time
            Summary = summary;
            Description = description;
            Category = category;
            Impact = impact;
            Likelihood = likelihood;
            AssigneeId = assigneeId;
            FollowUpDate = followUpDate;
            Response = response;

            UpdateStatus(status, timestamp);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    private void UpdateStatus(RiskStatus status, Instant timestamp)
    {
        if (Status == status) return;

        ClosedDate = status == RiskStatus.Closed ? timestamp : null;
        Status = status;
    }

    /// <summary>
    /// Create a new risk.
    /// </summary>
    /// <param name="summary"></param>
    /// <param name="description"></param>
    /// <param name="teamId"></param>
    /// <param name="reportedOn"></param>
    /// <param name="reportedById"></param>
    /// <param name="category"></param>
    /// <param name="impact"></param>
    /// <param name="likelihood"></param>
    /// <param name="assigneeId"></param>
    /// <param name="followUpDate"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static Risk Create(string summary, string? description, Guid? teamId, Instant reportedOn, Guid reportedById, RiskCategory category, RiskGrade impact, RiskGrade likelihood, Guid? assigneeId, LocalDate? followUpDate, string? response)
    {
        return new Risk(summary, description, teamId, reportedOn, reportedById, category, impact, likelihood, assigneeId, followUpDate, response);
    }

    /// <summary>
    /// Create a new risk.
    /// </summary>
    /// <param name="summary"></param>
    /// <param name="description"></param>
    /// <param name="teamId"></param>
    /// <param name="reportedOn"></param>
    /// <param name="reportedById"></param>
    /// <param name="status"></param>
    /// <param name="category"></param>
    /// <param name="impact"></param>
    /// <param name="likelihood"></param>
    /// <param name="assigneeId"></param>
    /// <param name="followUpDate"></param>
    /// <param name="response"></param>
    /// <param name="closedDate"></param>
    /// <returns></returns>
    public static Risk Import(string summary, string? description, Guid? teamId, Instant reportedOn, Guid reportedById, RiskStatus status, RiskCategory category, RiskGrade impact, RiskGrade likelihood, Guid? assigneeId, LocalDate? followUpDate, string? response, Instant? closedDate)
    {
        return new Risk()
        {
            Summary = summary,
            Description = description,
            TeamId = teamId,
            ReportedOn = reportedOn,
            ReportedById = reportedById,
            Status = status,
            Category = category,
            Impact = impact,
            Likelihood = likelihood,
            AssigneeId = assigneeId,
            FollowUpDate = followUpDate,
            Response = response,
            ClosedDate = closedDate,
        };
    }
}
