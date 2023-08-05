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
    private readonly int _maxPageSize = 100; // graph api max page size is 999

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
        var members = await _graphServiceClient.Groups[groupId]
            .TransitiveMembers
            .GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "manager($select=id)" };
                requestConfiguration.QueryParameters.Select = _selectOptions;
                requestConfiguration.QueryParameters.Filter = "accountEnabled eq true and usertype eq 'Member'";
                requestConfiguration.QueryParameters.Top = _maxPageSize;
            }, cancellationToken);

        List<User> users = new();
        if (members is null || members.Value is null)
        {
            _logger.LogWarning("GetGroupMembers:  No members found for group {GroupId} in Active Directory via Microsoft Graph", groupId);
            return users;
        }

        var pageIterator = PageIterator<DirectoryObject, DirectoryObjectCollectionResponse>
            .CreatePageIterator(
                _graphServiceClient,
                members,
                (d) =>
                {
                    users.Add((User)d);
                    return true;
                });

        await pageIterator.IterateAsync(cancellationToken);

        return users;
    }

    private async Task<List<User>> GetActiveDirectoryUsers(bool includeDisabled, CancellationToken cancellationToken)
    {
        var filter = includeDisabled
            ? "usertype eq 'Member'"
            : "accountEnabled eq true and usertype eq 'Member'";

        //https://docs.microsoft.com/en-us/graph/aad-advanced-queries?tabs=csharp
        var adUsers = await _graphServiceClient.Users
            .GetAsync(requestConfiguration =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "manager($select=id)" };
                requestConfiguration.QueryParameters.Select = _selectOptions;
                requestConfiguration.QueryParameters.Filter = filter;
                requestConfiguration.QueryParameters.Top = _maxPageSize;
            }, cancellationToken);

        List<User> users = new();
        if (adUsers is null || adUsers.Value is null)
        {
            _logger.LogWarning("GetActiveDirectoryUsers:  No users found in Active Directory via Microsoft Graph");
            return users;
        }

        var pageIterator = PageIterator<User, UserCollectionResponse>
            .CreatePageIterator(
                _graphServiceClient,
                adUsers,
                (u) =>
                {
                    users.Add(u);
                    return true;
                });

        await pageIterator.IterateAsync(cancellationToken);

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
