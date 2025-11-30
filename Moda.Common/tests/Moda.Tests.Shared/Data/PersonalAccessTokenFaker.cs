using Bogus;
using Moda.Common.Domain.Identity;
using NodaTime;

namespace Moda.Tests.Shared.Data;

public sealed class PersonalAccessTokenFaker : Faker<PersonalAccessToken>
{
    public PersonalAccessTokenFaker(Instant? timestamp = null)
    {
        var now = timestamp ?? SystemClock.Instance.GetCurrentInstant();

        CustomInstantiator(f =>
        {
            var name = f.Lorem.Sentence(3);
            var tokenHash = f.Random.Hash(40);
            var userId = f.Random.Guid().ToString();
            var employeeId = f.Random.Bool() ? f.Random.Guid() : (Guid?)null;
            var expiresAt = now.Plus(Duration.FromDays(f.Random.Int(30, 730))); // 30 days to 2 years
            var scopes = f.Random.Bool() ? null : $"[\"Permissions.{f.PickRandom("WorkItems", "Teams", "Projects")}.{f.PickRandom("View", "Create", "Update")}\"]";

            var result = PersonalAccessToken.Create(
                name,
                tokenHash,
                userId,
                employeeId,
                expiresAt,
                scopes,
                now
            );

            if (result.IsFailure)
                throw new InvalidOperationException($"Failed to create PersonalAccessToken: {result.Error}");

            return result.Value;
        });
    }

    public PersonalAccessTokenFaker WithRevokedToken(Guid revokedBy, Instant? revokedAt = null)
    {
        var now = revokedAt ?? SystemClock.Instance.GetCurrentInstant();

        FinishWith((f, token) =>
        {
            token.Revoke(revokedBy, now);
        });

        return this;
    }

    public PersonalAccessTokenFaker WithUsedToken(Instant? lastUsedAt = null)
    {
        var usedAt = lastUsedAt ?? SystemClock.Instance.GetCurrentInstant();

        FinishWith((f, token) =>
        {
            token.UpdateLastUsed(usedAt);
        });

        return this;
    }
}
