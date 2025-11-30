using Microsoft.EntityFrameworkCore;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Persistence;
using Moda.Common.Application.Validators;
using Moda.Common.Domain.Identity;
using NodaTime;

namespace Moda.Common.Application.Identity.PersonalAccessTokens;

/// <summary>
/// Command to create a new personal access token.
/// </summary>
public sealed record CreatePersonalAccessTokenCommand : ICommand<CreatePersonalAccessTokenResult>
{
    /// <summary>
    /// Gets the user-friendly name for this token.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the optional expiration date for the token.
    /// If not provided, defaults to 1 year from creation.
    /// </summary>
    public Instant? ExpiresAt { get; init; }

    /// <summary>
    /// Gets the optional scopes/permissions to limit this token to.
    /// If null or empty, token has full access (all user permissions).
    /// </summary>
    public string? Scopes { get; init; }
}

/// <summary>
/// Response DTO containing the created token details including the plaintext token.
/// This is the ONLY time the plaintext token is returned.
/// </summary>
public sealed record CreatePersonalAccessTokenResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Token { get; init; } = null!; // Plaintext token - only shown once!
    public Instant ExpiresAt { get; init; }
}

public sealed class CreatePersonalAccessTokenCommandValidator : CustomValidator<CreatePersonalAccessTokenCommand>
{
    private const int MaxTokensPerUser = 10;
    private const int DefaultExpirationDays = 365; // 1 year
    private const int MaxExpirationDays = 730; // 2 years

    private readonly IModaDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreatePersonalAccessTokenCommandValidator(
        IModaDbContext dbContext,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .MustAsync(BeUniqueTokenName)
                .WithMessage("You already have a token with this name.");

        RuleFor(x => x.ExpiresAt)
            .Must(BeValidExpirationDate)
                .WithMessage($"Expiration date must be between 1 day and {MaxExpirationDays} days ({MaxExpirationDays / 365} years) from now.")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x)
            .MustAsync(NotExceedTokenLimit)
                .WithMessage($"You have reached the maximum of {MaxTokensPerUser} active tokens. Please revoke an existing token before creating a new one.");

        RuleFor(x => x.Scopes)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.Scopes));
    }

    private async Task<bool> BeUniqueTokenName(string name, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        return !await _dbContext.PersonalAccessTokens
            .AnyAsync(t => t.UserId == userId && t.Name == name && t.RevokedAt == null, cancellationToken);
    }

    private bool BeValidExpirationDate(Instant? expiresAt)
    {
        if (!expiresAt.HasValue)
            return true;

        var now = _dateTimeProvider.Now;
        var minExpiration = now.Plus(Duration.FromDays(1));
        var maxExpiration = now.Plus(Duration.FromDays(MaxExpirationDays));

        return expiresAt.Value >= minExpiration && expiresAt.Value <= maxExpiration;
    }

    private async Task<bool> NotExceedTokenLimit(CreatePersonalAccessTokenCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId().ToString();
        var activeTokenCount = await _dbContext.PersonalAccessTokens
            .CountAsync(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > _dateTimeProvider.Now, cancellationToken);

        return activeTokenCount < MaxTokensPerUser;
    }
}

internal sealed class CreatePersonalAccessTokenCommandHandler : ICommandHandler<CreatePersonalAccessTokenCommand, CreatePersonalAccessTokenResult>
{
    private const int DefaultExpirationDays = 365; // 1 year

    private readonly IModaDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ITokenHashingService _tokenHashingService;
    private readonly ILogger<CreatePersonalAccessTokenCommandHandler> _logger;

    public CreatePersonalAccessTokenCommandHandler(
        IModaDbContext dbContext,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider,
        ITokenHashingService tokenHashingService,
        ILogger<CreatePersonalAccessTokenCommandHandler> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _dateTimeProvider = dateTimeProvider;
        _tokenHashingService = tokenHashingService;
        _logger = logger;
    }

    public async Task<Result<CreatePersonalAccessTokenResult>> Handle(CreatePersonalAccessTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUser.GetUserId().ToString();
            var employeeId = _currentUser.GetEmployeeId();
            var now = _dateTimeProvider.Now;

            // Calculate expiration date (default to 1 year if not provided)
            var expiresAt = request.ExpiresAt ?? now.Plus(Duration.FromDays(DefaultExpirationDays));

            // Generate the token
            var plaintextToken = _tokenHashingService.GenerateToken();
            var tokenHash = _tokenHashingService.HashToken(plaintextToken);

            // Create the domain entity
            var tokenResult = PersonalAccessToken.Create(
                name: request.Name,
                tokenHash: tokenHash,
                userId: userId,
                employeeId: employeeId,
                expiresAt: expiresAt,
                scopes: request.Scopes,
                timestamp: now
            );

            if (tokenResult.IsFailure)
            {
                return Result.Failure<CreatePersonalAccessTokenResult>(tokenResult.Error);
            }

            var token = tokenResult.Value;

            // Save to database
            await _dbContext.PersonalAccessTokens.AddAsync(token, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Personal access token created. UserId: {UserId}, TokenId: {TokenId}, TokenName: {TokenName}, ExpiresAt: {ExpiresAt}",
                userId, token.Id, token.Name, token.ExpiresAt);

            // Return the result with the plaintext token (ONLY time it's visible!)
            return Result.Success(new CreatePersonalAccessTokenResult
            {
                Id = token.Id,
                Name = token.Name,
                Token = plaintextToken,
                ExpiresAt = token.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;
            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);
            return Result.Failure<CreatePersonalAccessTokenResult>($"Failed to create personal access token: {ex.Message}");
        }
    }
}
