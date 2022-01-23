using SDVRP.App.Problem.Common.Nodes;
using SDVRP.App.Problem.Cost;

namespace SDVRP.App.Problem;

public record Problem
{
    public List<Node> Nodes { get; init; }

    public int VehicleCapacity { get; init; }

    public int MaxIterations { get; init; }
    
    public CostFunctionType CostFunctionType { get; init; }
}