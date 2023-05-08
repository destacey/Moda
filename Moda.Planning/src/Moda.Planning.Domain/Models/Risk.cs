using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Data;
using Moda.Common.Extensions;
using Moda.Planning.Domain.Enums;
using NodaTime;

namespace Moda.Planning.Domain.Models;
public class Risk : BaseAuditableEntity<Guid>
{
    private string _summary = default!;
    private string? _description;
    private string? _response;

    private Risk() { }

    private Risk(string summary, string? description, Guid? teamId, Instant reportedOn, Guid reportedBy, RiskCategory category, RiskGrade impact, RiskGrade likelihood, Guid? assigneeId, LocalDate followUpDate, string? response)
    {
        Summary = summary;
        Description = description;
        TeamId = teamId;
        ReportedOn = reportedOn;
        ReportedBy = reportedBy;
        Category = category;
        Impact = impact;
        Likelihood = likelihood;
        AssigneeId = assigneeId;
        FollowUpDate = followUpDate;
        Response = response;

        Status = RiskStatus.Open;
    }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; private set; }

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

    public Guid? TeamId { get; private set; }

    public Instant ReportedOn { get; private set; }

    public Guid ReportedBy { get; private set; }

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

    public LocalDate? FollowUpDate { get; private set; }

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
    /// <param name="teamId"></param>
    /// <param name="reportedOn"></param>
    /// <param name="reportedBy"></param>
    /// <param name="status"></param>
    /// <param name="category"></param>
    /// <param name="impact"></param>
    /// <param name="likelihood"></param>
    /// <param name="assigneeId"></param>
    /// <param name="followUpDate"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public Result Update(string summary, string? description, Guid? teamId, Instant reportedOn, Guid reportedBy, RiskStatus status, RiskCategory category, RiskGrade impact, RiskGrade likelihood, Guid? assigneeId, LocalDate followUpDate, string? response)
    {
        try
        {
            Summary = summary;
            Description = description;
            TeamId = teamId;
            ReportedOn = reportedOn;
            ReportedBy = reportedBy;
            Status = status;
            Category = category;
            Impact = impact;
            Likelihood = likelihood;
            AssigneeId = assigneeId;
            FollowUpDate = followUpDate;
            Response = response;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>
    /// Create a new risk.
    /// </summary>
    /// <param name="summary"></param>
    /// <param name="description"></param>
    /// <param name="teamId"></param>
    /// <param name="reportedOn"></param>
    /// <param name="reportedBy"></param>
    /// <param name="category"></param>
    /// <param name="impact"></param>
    /// <param name="likelihood"></param>
    /// <param name="assigneeId"></param>
    /// <param name="followUpDate"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static Risk Create(string summary, string? description, Guid? teamId, Instant reportedOn, Guid reportedBy, RiskCategory category, RiskGrade impact, RiskGrade likelihood, Guid? assigneeId, LocalDate followUpDate, string? response)
    {
        return new Risk(summary, description, teamId, reportedOn, reportedBy, category, impact, likelihood, assigneeId, followUpDate, response);
    }
}
