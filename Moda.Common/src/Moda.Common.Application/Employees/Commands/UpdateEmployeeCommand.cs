﻿using Moda.Common.Application.Persistence;
using Moda.Common.Application.Validators;
using Moda.Common.Models;

namespace Moda.Common.Application.Employees.Commands;
public sealed record UpdateEmployeeCommand : ICommand<int>
{
    public UpdateEmployeeCommand(Guid id, PersonName name, string employeeNumber, Instant? hireDate, EmailAddress email, string? jobTitle, string? department, string? officeLocation, Guid? managerId)
    {
        Id = id;
        Name = name;
        EmployeeNumber = employeeNumber;
        HireDate = hireDate;
        Email = email;
        JobTitle = jobTitle;
        Department = department;
        OfficeLocation = officeLocation;
        ManagerId = managerId;
    }

    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; }

    /// <summary>Gets the employee name.</summary>
    /// <value>The employee name.</value>
    public PersonName Name { get; }

    /// <summary>Gets the employee identifier.</summary>
    /// <value>The employee identifier.</value>
    public string EmployeeNumber { get; }

    /// <summary>Gets the hire date.</summary>
    /// <value>The hire date.</value>
    public Instant? HireDate { get; }

    /// <summary>Gets the email.</summary>
    /// <value>The email.</value>
    public EmailAddress Email { get; }

    /// <summary>Gets the job title.</summary>
    /// <value>The job title.</value>
    public string? JobTitle { get; }

    /// <summary>Gets the department.</summary>
    /// <value>The department.</value>
    public string? Department { get; }

    /// <summary>Gets the office location.</summary>
    /// <value>The office location.</value>
    public string? OfficeLocation { get; }

    /// <summary>Gets the manager identifier.</summary>
    /// <value>The manager identifier.</value>
    public Guid? ManagerId { get; }
}

public sealed class UpdateEmployeeCommandValidator : CustomValidator<UpdateEmployeeCommand>
{
    private readonly IModaDbContext _modaDbContext;

    public UpdateEmployeeCommandValidator(IModaDbContext modaDbContext)
    {
        _modaDbContext = modaDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Name)
            .NotNull()
            .SetValidator(new PersonNameValidator());

        RuleFor(e => e.EmployeeNumber)
            .NotEmpty()
            .MaximumLength(256)
            .MustAsync(async (cmd, employeeNumber, cancellationToken) => await BeUniqueEmployeeNumber(cmd.Id, employeeNumber, cancellationToken))
                .WithMessage("The EmployeeNumber already exists.");

        RuleFor(e => e.Email)
            .NotNull()
            .SetValidator(new EmailAddressValidator());

        RuleFor(e => e.JobTitle)
            .MaximumLength(256);

        RuleFor(e => e.Department)
            .MaximumLength(256);

        RuleFor(e => e.OfficeLocation)
            .MaximumLength(256);
    }

    public async Task<bool> BeUniqueEmployeeNumber(Guid id, string employeeNumber, CancellationToken cancellationToken)
    {
        return await _modaDbContext.Employees
            .Where(e => e.Id != id)
            .AllAsync(e => e.EmployeeNumber != employeeNumber, cancellationToken);
    }
}

internal sealed class UpdateEmployeeCommandHandler : ICommandHandler<UpdateEmployeeCommand, int>
{
    private readonly IModaDbContext _modaDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateEmployeeCommandHandler> _logger;

    public UpdateEmployeeCommandHandler(IModaDbContext modaDbContext, IDateTimeService dateTimeService, ILogger<UpdateEmployeeCommandHandler> logger)
    {
        _modaDbContext = modaDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _modaDbContext.Employees
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (employee is null)
                return Result.Failure<int>("Employee not found.");

            // verify the manager exists
            if (request.ManagerId.HasValue
                && employee.ManagerId != request.ManagerId
                && await _modaDbContext.Employees.AllAsync(e => e.Id != request.ManagerId.Value, cancellationToken))
            {
                _logger.LogWarning("Moda Request: Unable to find manager {ManagerId} while updating employee {EmployeeId}", request.ManagerId.Value, request.Id);
                return Result.Failure<int>("Manager not found.");
            }

            var updateResult = employee.Update(
                request.Name,
                request.EmployeeNumber,
                request.HireDate,
                request.Email,
                request.JobTitle,
                request.Department,
                request.OfficeLocation,
                request.ManagerId,
                employee.IsActive,  // this command should not change IsActive
                _dateTimeService.Now
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _modaDbContext.Entry(employee).ReloadAsync(cancellationToken);
                employee.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

            await _modaDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(employee.Key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}

