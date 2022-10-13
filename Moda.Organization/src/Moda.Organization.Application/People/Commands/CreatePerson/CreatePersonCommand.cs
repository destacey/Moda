using CSharpFunctionalExtensions;
using FluentValidation;
using Moda.Organization.Application.People.Queries.GetPersonByKey;

namespace Moda.Organization.Application.People.Commands.CreatePerson;
public sealed record class CreatePersonCommand : ICommand<Guid>
{
    public CreatePersonCommand(string key)
    {
        Key = key.Trim();
    }
    public string Key { get; } = default!;
}

public sealed class CreatePersonCommandValidator : CustomValidator<CreatePersonCommand>
{
    private readonly IReadRepository<Person> _readRepository;

    public CreatePersonCommandValidator(IReadRepository<Person> readRepository)
    {
        _readRepository = readRepository;

        RuleFor(q => q.Key)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(BeUniqueKey).WithMessage("The Key already exists.");
    }

    public async Task<bool> BeUniqueKey(string key, CancellationToken cancellationToken)
    {
        return await _readRepository.AnyAsync(new PersonByKeySpec(key), cancellationToken);
    }
}

public sealed class CreatePersonCommandHandler : ICommandHandler<CreatePersonCommand, Guid>
{
    private readonly IRepositoryWithEvents<Person> _repository;

    public CreatePersonCommandHandler(IRepositoryWithEvents<Person> repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = new Person(request.Key);

        await _repository.AddAsync(person, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return person.Id;
    }
}
