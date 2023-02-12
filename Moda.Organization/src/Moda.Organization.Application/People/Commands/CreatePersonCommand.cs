using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.People.Commands;
public sealed record CreatePersonCommand : ICommand<Guid>
{
    public CreatePersonCommand(string key)
    {
        Key = key.Trim();
    }
    public string Key { get; } = default!;
}

public sealed class CreatePersonCommandValidator : CustomValidator<CreatePersonCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public CreatePersonCommandValidator(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;

        RuleFor(q => q.Key).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(BeUniqueKey).WithMessage("The Key already exists.");
    }

    public async Task<bool> BeUniqueKey(string key, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.People.AllAsync(x => x.Key != key, cancellationToken);
    }
}

internal sealed class CreatePersonCommandHandler : ICommandHandler<CreatePersonCommand, Guid>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreatePersonCommandHandler> _logger;

    public CreatePersonCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<CreatePersonCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var person = Person.Create(request.Key, _dateTimeService.Now);

            await _organizationDbContext.People.AddAsync(person, cancellationToken);

            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(person.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
