namespace SDVRP.App.Problem.Common.Nodes;

public static class NodeExtensions
{
    public static Node StartingNode(this IEnumerable<Node> nodes) => nodes.Single(x => x.Id == 0);
    
    public static double DistanceTo(this Position position1, Position position2)
        => Math.Sqrt(Math.Pow(position1.Y - position2.Y, 2) + Math.Pow(position1.X - position2.X, 2));

    public static Node ClosestTo(this IEnumerable<Node> nodes, Position position)
        => nodes
            .OrderBy(node => node.DistanceTo(position))
            .First();

    public static IEnumerable<Node> WithDemand(this IEnumerable<Node> nodes) => nodes.Where(node => node.Demand > 0);

    public static double CalculateCost(this IReadOnlyList<Node> nodes, Func<double, double> costPerDistanceUnitFunction)
    {
        return nodes
            .LinkNodes()
            .CalculateDistanceBetweenNodes()
            .CalculateCost(costPerDistanceUnitFunction);
    }

    private static List<(Node Previous, Node Current)> LinkNodes(this IReadOnlyList<Node> nodes)
        => nodes.Zip(nodes.Skip(1)).ToList();

    private static List<double> CalculateDistanceBetweenNodes(this IReadOnlyList<(Node Previous, Node Current)> nodes)
        => nodes
            .Select(nodesPair => nodesPair.Previous.DistanceTo(nodesPair.Current))
            .ToList();

    private static double CalculateCost(
        this IEnumerable<double> distancesBetweenNodes,
        Func<double, double> costPerDistanceUnitFunc)
        => distancesBetweenNodes.Aggregate(
            0d,
            (currentCost, distance) => currentCost + costPerDistanceUnitFunc(distance));
}
