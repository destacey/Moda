using Moda.Integrations.AzureDevOps.Models.Projects;

namespace Moda.Integrations.AzureDevOps.Extensions;
internal static class IterationNodeDtoExtensions
{
    public static IterationNodeDto SetTeamIds(this IterationNodeDto root, Dictionary<Guid,Guid> iterationTeamMappings)
    {
        Stack<IterationNodeDtoStackItem> stack = new();
        stack.Push(IterationNodeDtoStackItem.Create(root, null));

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            current.Iteration.TeamId = iterationTeamMappings.TryGetValue(current.Iteration.Identifier, out var teamId) ? teamId : current.ParentTeamId;

            foreach (var child in current.Iteration.Children ?? [])
            {
                stack.Push(IterationNodeDtoStackItem.Create(child, current.Iteration.TeamId));
            }
        }

        return root;
    }

    private sealed record IterationNodeDtoStackItem
    {
        public IterationNodeDto Iteration { get; set; } = null!;
        public Guid? ParentTeamId { get; set; }

        public static IterationNodeDtoStackItem Create(IterationNodeDto iterationNodeDto, Guid? parentTeamId) =>
            new()
            {
                Iteration = iterationNodeDto,
                ParentTeamId = parentTeamId
            };
    }
}
