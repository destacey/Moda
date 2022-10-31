using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.Organization.Application.People.EventHandlers;

/// <summary>
/// Create a Person when the EmployeeId does not exist as a key in an existing Person.
/// </summary>
/// <seealso cref="Moda.Common.Application.Events.EventNotificationHandler&lt;Moda.Common.Domain.Events.EntityCreatedEvent&lt;Moda.Organization.Domain.Models.Employee&gt;&gt;" />
public sealed class EmployeeCreatedEventHandler : EventNotificationHandler<EntityCreatedEvent<Employee>>
{
    private readonly string _notificationName = "EmployeeCreatedEvent";

    private readonly ILogger<EmployeeCreatedEventHandler> _logger;
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;

    public EmployeeCreatedEventHandler(ILogger<EmployeeCreatedEventHandler> logger, IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService)
    {
        _logger = logger;
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
    }

    public override async Task Handle(EntityCreatedEvent<Employee> @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{NotificationName} Triggered", _notificationName);

        // Do nothing if the key already exists.
        if (await _organizationDbContext.People.AnyAsync(p => p.Key == @event.Entity.EmployeeNumber, cancellationToken))
            return;
        
        try
        {
            var person = Person.CreateFromExisting(@event.Entity.Id, @event.Entity.EmployeeNumber, _dateTimeService.Now);

            await _organizationDbContext.People.AddAsync(person, cancellationToken);

            await _organizationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Moda Notification: Exception for Notification {Name} {@Employee}", _notificationName, @event.Entity);
        }
    }
}
