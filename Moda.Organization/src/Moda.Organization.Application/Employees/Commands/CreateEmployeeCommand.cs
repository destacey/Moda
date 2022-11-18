﻿using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Moda.Organization.Application.Employees.Commands;
public sealed record CreateEmployeeCommand : ICommand<int>
{
    public CreateEmployeeCommand(PersonName name, string employeeNumber, LocalDate? hireDate, EmailAddress email, string? jobTitle, string? department, Guid? managerId)
    {
        Name = name;
        EmployeeNumber = employeeNumber;
        HireDate = hireDate;
        Email = email;
        JobTitle = jobTitle;
        Department = department;
        ManagerId = managerId;
    }

    /// <summary>Gets the employee name.</summary>
    /// <value>The employee name.</value>
    public PersonName Name { get; } = null!;

    /// <summary>Gets the employee identifier.</summary>
    /// <value>The employee identifier.</value>
    public string EmployeeNumber { get; }

    /// <summary>Gets the hire date.</summary>
    /// <value>The hire date.</value>
    public LocalDate? HireDate { get; }

    /// <summary>Gets the email.</summary>
    /// <value>The email.</value>
    public EmailAddress Email { get; } = null!;

    /// <summary>Gets the job title.</summary>
    /// <value>The job title.</value>
    public string? JobTitle { get; }

    /// <summary>Gets the department.</summary>
    /// <value>The department.</value>
    public string? Department { get; }

    /// <summary>Gets the manager identifier.</summary>
    /// <value>The manager identifier.</value>
    public Guid? ManagerId { get; }
}

public sealed class CreateEmployeeCommandValidator : CustomValidator<CreateEmployeeCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public CreateEmployeeCommandValidator(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Name)
            .NotNull()
            .SetValidator(new PersonNameValidator());

        RuleFor(e => e.EmployeeNumber)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(BeUniqueEmployeeId).WithMessage("The EmployeeId already exists.");

        RuleFor(e => e.Email)
            .NotNull()
            .SetValidator(new EmailAddressValidator());

        RuleFor(e => e.JobTitle)
            .MaximumLength(256);

        RuleFor(e => e.Department)
            .MaximumLength(256);
    }

    public async Task<bool> BeUniqueEmployeeId(string employeeNumber, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.Employees.AllAsync(x => x.EmployeeNumber != employeeNumber, cancellationToken);
    }
}

internal sealed class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, int>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateEmployeeCommandHandler> _logger;

    public CreateEmployeeCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<CreateEmployeeCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var personId = await _organizationDbContext.People
                .Where(p => p.Key == request.EmployeeNumber)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            // verify the manager exists
            var managerId = request.ManagerId;
            if (managerId.HasValue 
                && await _organizationDbContext.Employees.AllAsync(e => e.Id != request.ManagerId, cancellationToken))
            {
                managerId = null;
            }

            var employee = Employee.Create(
                personId ?? Guid.NewGuid(),
                request.Name,
                request.EmployeeNumber,
                request.HireDate,
                request.Email,
                request.JobTitle,
                request.Department,
                managerId,
                _dateTimeService.Now
                );

            await _organizationDbContext.Employees.AddAsync(employee, cancellationToken);

            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(employee.LocalId);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
