namespace SDVRP.App.Problem.Common.Nodes;

public record VisitedNode
{
    public int Load { get; init; }

    public Node Node { get; init; } = new();
}
