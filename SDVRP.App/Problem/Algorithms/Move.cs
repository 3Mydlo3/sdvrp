using SDVRP.App.Problem.Common.Nodes;
using SDVRP.App.Problem.Common.Vehicles;

namespace SDVRP.App.Problem.Algorithms;

internal record Move
{
    public Vehicle Vehicle { get; init; }
    
    public Node Node { get; init; }
}
