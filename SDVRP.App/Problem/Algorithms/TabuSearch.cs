using SDVRP.App.Problem.Common;
using SDVRP.App.Problem.Common.Nodes;
using SDVRP.App.Problem.Common.Vehicles;

namespace SDVRP.App.Problem.Algorithms;

public class TabuSearch
{
    private readonly VehicleFactory _vehicleFactory;
    private readonly GreedySolutionCreator _greedySolutionCreator;

    private HashSet<KeyValuePair<int, int>> TabuListSet = new();
    private Queue<KeyValuePair<int, int>> TabuListQueue = new();

    public TabuSearch(
        VehicleFactory vehicleFactory,
        GreedySolutionCreator greedySolutionCreator)
    {
        _vehicleFactory = vehicleFactory;
        _greedySolutionCreator = greedySolutionCreator;
    }

    public Solution Solve(IReadOnlyList<Node> nodes, int maxIterations)
    {
        var initialSolution = _greedySolutionCreator.CreateInitialSolution(nodes);

        return initialSolution;
    }
}
