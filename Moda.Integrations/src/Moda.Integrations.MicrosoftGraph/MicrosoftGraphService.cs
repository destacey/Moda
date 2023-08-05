using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Moda.Common.Application.Interfaces;
using Moda.Integrations.MicrosoftGraph.Model;

namespace Moda.Integrations.MicrosoftGraph;

public sealed class MicrosoftGraphService : IExternalEmployeeDirectoryService
{
    private readonly string[] _selectOptions = new string[] { "id", "userPrincipalName", "userType", "accountEnabled", "givenName", "surname", "jobTitle", "department", "officeLocation", "mail", "manager", "employeeHireDate" };

    private readonly GraphServiceClient _graphServiceClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MicrosoftGraphService> _logger;

    public MicrosoftGraphService(GraphServiceClient graphServiceClient, IConfiguration configuration, ILogger<MicrosoftGraphService> logger)
    {
        _graphServiceClient = graphServiceClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<IExternalEmployee>>> GetEmployees(CancellationToken cancellationToken)
    {
        try
        {
            string? allEmployeesGroup = _configuration["GraphApi:AllEmployeesGroupObjectId"];
            var users = string.IsNullOrWhiteSpace(allEmployeesGroup)
                ? await GetActiveDirectoryUsers(true, cancellationToken)
                : await GetGroupMembers(allEmployeesGroup, cancellationToken);

            if (users is null || !users.Any())
                return Result.Failure<IEnumerable<IExternalEmployee>>("No employees found in Active Directory via Microsoft Graph");

            _logger.LogInformation("Found {UserCount} users in Active Directory via Microsoft Graph", users.Count);

            users = users.Where(u => !string.IsNullOrWhiteSpace(u.Id) && !string.IsNullOrEmpty(u.GivenName) && !string.IsNullOrEmpty(u.Surname)).ToList();
            List<AzureAdEmployee> employees = new(users.Count);
            foreach (var user in users)
            {
                employees.Add(new AzureAdEmployee(user));
            }

            _logger.LogInformation("Returning {EmployeeCount} employees from Active Directory via Microsoft Graph", employees.Count);
            return Result.Success((IEnumerable<IExternalEmployee>)employees);
        }
        catch (Exception ex)
        {
            string message = "Error getting employees from Active Directory via Microsoft Graph";
            _logger.LogError(ex, message);
            return Result.Failure<IEnumerable<IExternalEmployee>>(message);
        }
    }

    /// <summary>
    /// Gets a list of users that are members (direct or indirect) of a group.
    /// </summary>
    /// <param name="groupId">Azure Ad Group Object Id</param>
    /// <returns></returns>
    private async Task<List<User>> GetGroupMembers(string groupId, CancellationToken cancellationToken)
    {
        // TODO handle paging
        var members = await _graphServiceClient.Groups[groupId]
            .TransitiveMembers
            .GetAsync(requestConfiguration =>
            {
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                requestConfiguration.QueryParameters.Count = true;
                requestConfiguration.QueryParameters.Expand = new string[] { "manager($select=id)" };
                requestConfiguration.QueryParameters.Select = _selectOptions;
                requestConfiguration.QueryParameters.Filter = "accountEnabled eq true and usertype eq 'Member'";
            }, cancellationToken);

        return members?.OdataCount > 0 ? members.Value!.Select(m => (User)m).ToList() : new List<User>();
    }

    private async Task<List<User>> GetActiveDirectoryUsers(bool includeDisabled, CancellationToken cancellationToken)
    {
        var filter = includeDisabled
            ? "usertype eq 'Member'"
            : "accountEnabled eq true and usertype eq 'Member'";

        var users = new List<User>();

        // TODO handle paging
        //https://docs.microsoft.com/en-us/graph/aad-advanced-queries?tabs=csharp
        var adUsers = await _graphServiceClient.Users//.GetAsync();
            .GetAsync(requestConfiguration =>
            {
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                requestConfiguration.QueryParameters.Count = true;
                requestConfiguration.QueryParameters.Expand = new string[] { "manager($select=id)" };
                requestConfiguration.QueryParameters.Select = _selectOptions;
                requestConfiguration.QueryParameters.Filter = filter;
            }, cancellationToken);
        _logger.LogInformation("Initial Graph request complete, found {UserCount} users", adUsers?.OdataCount ?? 0);
        _logger.LogInformation("NextLink value is {NextLink}", adUsers?.OdataNextLink ?? "null");
        users.AddRange(adUsers?.Value!);
        var nextLink = adUsers?.OdataNextLink;

        while (nextLink != null)
        {
            var req = _graphServiceClient.Users.ToGetRequestInformation();
            req.QueryParameters.Add("%24skiptoken", adUsers.OdataNextLink);
            var result = await _graphServiceClient.RequestAdapter.SendAsync(req, UserCollectionResponse.CreateFromDiscriminatorValue, cancellationToken: cancellationToken);
            _logger.LogInformation("NextLink request complete, found {UserCount} users", result.OdataCount);
            users.AddRange(adUsers.Value!);
            nextLink = result.OdataNextLink;
            _logger.LogInformation("New NextLink value is {NextLink}", nextLink);
        }

        return users;
    }

    private async Task<DirectoryObject?> GetUserManager(string userId, CancellationToken cancellationToken)
    {
        try
        {
            var directoryObject = await _graphServiceClient.Users[userId].Manager
            .GetAsync(requestConfiguration =>
            {
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                requestConfiguration.QueryParameters.Select = _selectOptions;
            }, cancellationToken);

            return directoryObject;
        }
        catch (ODataError ex)
        {
            // Msft Graph throws an error rather than returning null
            _logger.LogDebug(ex, "Micrsoft Graph ServiceException:  The resource 'manager' does not exist for userId {UserId}", userId);
        }

        return null;
    }
}
