using Moda.Common.Application.Persistence;

namespace Moda.Common.Application.Employees.Commands;

public sealed record DeleteEmployeeCommand(Guid Id) : ICommand;

public sealed class DeleteEmployeeCommandValidator : CustomValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal sealed class DeleteEmployeeCommandHandler(IModaDbContext modaDbContext, ILogger<DeleteEmployeeCommandHandler> logger) : ICommandHandler<DeleteEmployeeCommand>
{
    private const string AppRequestName = nameof(DeleteEmployeeCommand);

    private readonly IModaDbContext _modaDbContext = modaDbContext;
    private readonly ILogger<DeleteEmployeeCommandHandler> _logger = logger;

    public async Task<Result> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _modaDbContext.Employees
                .Include(e => e.DirectReports)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (employee is null) {
                _logger.LogWarning("Employee with ID {EmployeeId} not found for deletion.", request.Id);
                return Result.Failure("Employee not found.");
            }

            _modaDbContext.Employees.Remove(employee);

            await _modaDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Employee {EmployeeId} deleted successfully. Name: {EmployeeName}, Email: {EmployeeEmail}", request.Id, employee.Name, employee.Email);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}