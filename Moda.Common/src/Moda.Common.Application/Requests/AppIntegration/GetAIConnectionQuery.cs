using Moda.Common.Application.Dtos;

namespace Moda.Common.Application.Requests.AppIntegration;

public sealed record GetAIConnectionQuery() : IQuery<EnabledAIConnectionDto>;