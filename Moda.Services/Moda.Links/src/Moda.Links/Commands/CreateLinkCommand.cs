using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Validation;
using Moda.Links.Models;

namespace Moda.Links.Commands;
public sealed record CreateLinkCommand(Guid ObjectId, string Name, string Url): ICommand<Guid>;

public sealed class CreateLinkCommandValidator : CustomValidator<CreateLinkCommand>
{
    public CreateLinkCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(l => l.ObjectId)
            .NotEmpty();

        RuleFor(l => l.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(l => l.Url)
            .NotEmpty()
            .MaximumLength(1024);
    }
}

internal sealed class CreateLinkCommandHandler : ICommandHandler<CreateLinkCommand, Guid>
{
    private readonly ILinksDbContext _linksDbContext;
    private readonly ILogger<CreateLinkCommandHandler> _logger;

    public CreateLinkCommandHandler(ILinksDbContext linksDbContext, ILogger<CreateLinkCommandHandler> logger)
    {
        _linksDbContext = linksDbContext;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateLinkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var link = Link.Create(request.ObjectId, request.Name, request.Url);

            await _linksDbContext.Links.AddAsync(link, cancellationToken);

            await _linksDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(link.Id);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<Guid>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
