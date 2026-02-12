using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models.OpenAI;

public class OpenAIConnection : Connection<OpenAIConnectionConfiguration>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private OpenAIConnection() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private OpenAIConnection(
        string name,
        string? description,
        bool configurationIsValid,
        OpenAIConnectionConfiguration configuration)
    {
        Name = name;
        Description = description;
        IsValidConfiguration = configurationIsValid;
        Connector = Moda.Common.Domain.Enums.AppIntegrations.Connector.OpenAI;
        Configuration = Guard.Against.Null(configuration, nameof(Configuration));
    }

    public override OpenAIConnectionConfiguration Configuration { get; protected set; }

    public override bool HasActiveIntegrationObjects => IsValidConfiguration;

    public Result Update(string name, string? description, string apiKey, string organization, string project, string modelName, bool configurationIsValid, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));

            var newName = Guard.Against.NullOrWhiteSpace(name, nameof(name)).Trim();
            var newDescription = description?.NullIfWhiteSpacePlusTrim();
            var newApiKey = Guard.Against.NullOrWhiteSpace(apiKey, nameof(apiKey)).Trim();
            var newOrganizationId = Guard.Against.NullOrWhiteSpace(organization, nameof(organization)).Trim();
            var newProjectId = Guard.Against.NullOrWhiteSpace(project, nameof(project)).Trim();
            var newModelName = Guard.Against.NullOrWhiteSpace(modelName, nameof(modelName)).Trim();

            if (!UpdateValuesChanged(newName, newDescription, newApiKey, newOrganizationId, newProjectId, newModelName, configurationIsValid))
                return Result.Success();

            Name = newName;
            Description = newDescription;
            IsValidConfiguration = configurationIsValid;

            Configuration.OrganizationId = newOrganizationId;
            Configuration.ApiKey = newApiKey;
            Configuration.ProjectId = newProjectId;
            Configuration.ModelName = newModelName;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    private bool UpdateValuesChanged(
        string name,
        string? description,
        string apiKey,
        string organization,
        string project,
        string modelName,
        bool configurationIsValid)
    {
        return Name != name.Trim()
            || Description != description.NullIfWhiteSpacePlusTrim()
            || Configuration.ApiKey != apiKey.Trim()
            || Configuration.OrganizationId != organization.Trim()
            || Configuration.ProjectId != project.Trim()
            || Configuration.ModelName != modelName.Trim()
            || IsValidConfiguration != configurationIsValid; // Assuming OpenAIConnectionConfiguration has an appropriate Equals method
    }

    public static OpenAIConnection Create(
        string name,
        string? description,
        OpenAIConnectionConfiguration configuration,
        Instant timestamp)
    {
        var connection = new OpenAIConnection(
            name,
            description,
            configuration.ApiKey.Length > 0 && configuration.ModelName.Length > 0,
            configuration);

        connection.AddDomainEvent(EntityCreatedEvent.WithEntity(connection, timestamp));

        return connection;
    }
}
