using System.Collections.Immutable;
using SDVRP.App.Problem.Common.Nodes;

namespace SDVRP.App.Problem.Common.Vehicles;

public record Vehicle
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int Id { get; init; }
    
    public int Capacity { get; init; }

    public int FreeCapacity => Capacity - LoadedCapacity;

    public int OverloadCapacity => -FreeCapacity;

    public ImmutableList<VisitedNode> Path { get; init; } = ImmutableList<VisitedNode>.Empty;
    
    private int LoadedCapacity => Path.Sum(x => x.Load);

    public (Vehicle LoadedVehicle, Node LoadedNode) Load(Node node)
    {
        var vehicle = this;
        var loadedFromNode = Math.Min(FreeCapacity, node.Demand);
        return (
            vehicle with
            {
                Path = vehicle.Path.Add(new VisitedNode {Load = loadedFromNode, Node = node})
            },
            node with
            {
                Demand = node.Demand - loadedFromNode
            });
    }

    public double CalculateCost(Func<double, double> costPerDistanceUnitFunc)
        => Path
            .Select(x => x.Node)
            .ToList()
            .CalculateCost(costPerDistanceUnitFunc);
}