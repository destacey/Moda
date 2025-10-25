﻿using Moda.Common.Application.Models;
using Moda.Common.Domain.Interfaces.Planning.Iterations;
using Moda.Work.Application.Persistence;

namespace Moda.Work.Application.WorkIterations.Commands;
public sealed record SyncWorkIterationsCommand(IEnumerable<ISimpleIteration> Iterations) : ICommand, ILongRunningRequest;

internal sealed class SyncWorkIterationsCommandHandler(
    IWorkDbContext workDbContext,
    ILogger<SyncWorkIterationsCommandHandler> logger)
    : ICommandHandler<SyncWorkIterationsCommand>
{
    private const string AppRequestName = nameof(SyncWorkIterationsCommand);

    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly ILogger<SyncWorkIterationsCommandHandler> _logger = logger;

    public async Task<Result> Handle(SyncWorkIterationsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Iterations == null || !request.Iterations.Any())
            {
                _logger.LogInformation("No iterations to sync.");
                return Result.Success();
            }

            int createCount = 0;
            int updateCount = 0;
            int deleteCount = 0;

            var existingIterations = await _workDbContext.WorkIterations
                .ToListAsync(cancellationToken);
            var existingIds = existingIterations.Select(x => x.Id).ToHashSet();

            // Handle deletes
            var deleteIds = existingIds.Except(request.Iterations.Select(x => x.Id)).ToList();
            if (deleteIds.Count != 0)
            {
                var iterationsToDelete = existingIterations.Where(x => deleteIds.Contains(x.Id)).ToList();
                _workDbContext.WorkIterations.RemoveRange(iterationsToDelete);
                deleteCount = iterationsToDelete.Count;
            }
            
            // Handle creates and updates
            foreach (var iteration in request.Iterations)
            {
                var existingIteration = existingIterations.FirstOrDefault(x => x.Id == iteration.Id);
                if (existingIteration == null)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("Creating new Work iteration {IterationId}.", iteration.Id);

                    var newIteration = new WorkIteration(iteration);

                    await _workDbContext.WorkIterations.AddAsync(newIteration, cancellationToken);
                    createCount++;
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("Updating existing Work iteration {IterationId}.", iteration.Id);

                    existingIteration.Update(iteration);
                    updateCount++;
                }
            }

            await _workDbContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Sync Work iterations completed. Created: {CreateCount}, Updated: {UpdateCount}, Deleted: {DeleteCount}.",
                createCount, updateCount, deleteCount);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command.", AppRequestName);
            return Result.Failure<ObjectIdAndKey>($"Error handling {AppRequestName} command.");
        }
    }
}
