using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Moda.Integrations.MicrosoftGraph.Model;
using Moda.Organization.Application.Interfaces;

namespace Moda.Integrations.MicrosoftGraph;

public sealed class MicrosoftGraphService : IExternalEmployeeDirectoryService
{
    private readonly string[] _selectOptions = new string[] { "id", "userPrincipalName", "accountEnabled", "givenName", "surname", "jobTitle", "department", "officeLocation", "mail", "manager", "employeeHireDate" };

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

            List<AzureAdEmployee> employees = new(users.Count);
            foreach (var user in users.Where(u => !string.IsNullOrWhiteSpace(u.Id)))
            {
                // TODO move this to a single operation for the entire list of users
                var manager = await GetUserManager(user.Id!, cancellationToken);
                user.Manager = manager;
                employees.Add(new AzureAdEmployee(user));
            }

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
    private async Task<IReadOnlyList<User>> GetGroupMembers(string groupId, CancellationToken cancellationToken)
    {
        // TODO handle paging
        var members = await _graphServiceClient.Groups[groupId]
            .TransitiveMembers
            .GetAsync(requestConfiguration =>
            {
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                requestConfiguration.QueryParameters.Count = true;
                requestConfiguration.QueryParameters.Select = _selectOptions;
                requestConfiguration.QueryParameters.Filter = "accountEnabled eq true and usertype eq 'Member' and givenName ne null and surname ne null";
            }, cancellationToken);

        return members?.OdataCount > 0 ? members.Value!.Select(m => (User)m).ToList() : new List<User>();
    }

    private async Task<IReadOnlyList<User>> GetActiveDirectoryUsers(bool includeDisabled, CancellationToken cancellationToken)
    {
        var filter = includeDisabled
            ? "usertype eq 'Member' and givenName ne null and surname ne null"
            : "accountEnabled eq true and usertype eq 'Member' and givenName ne null and surname ne null";

        // TODO handle paging
        //https://docs.microsoft.com/en-us/graph/aad-advanced-queries?tabs=csharp
        var adUsers = await _graphServiceClient.Users//.GetAsync();
            .GetAsync(requestConfiguration =>
            {
                requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                requestConfiguration.QueryParameters.Count = true;
                requestConfiguration.QueryParameters.Select = _selectOptions;
                requestConfiguration.QueryParameters.Filter = filter;
            }, cancellationToken);

        return adUsers?.OdataCount > 0 ? adUsers.Value!.ToList() : new List<User>();
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
