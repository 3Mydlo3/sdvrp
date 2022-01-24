using SDVRP.App.Problem.Common;
using SDVRP.App.Problem.Common.Nodes;
using SDVRP.App.Problem.Common.Vehicles;
using SDVRP.App.Problem.Extensions;

namespace SDVRP.App.Problem.Algorithms;

public class TabuSearch
{
    private readonly VehicleFactory _vehicleFactory;
    private readonly GreedySolutionCreator _greedySolutionCreator;

    public TabuSearch(
        VehicleFactory vehicleFactory,
        GreedySolutionCreator greedySolutionCreator)
    {
        _vehicleFactory = vehicleFactory;
        _greedySolutionCreator = greedySolutionCreator;
    }

    public Solution Solve(IReadOnlyList<Node> nodes, int maxIterations)
    {
        var bestSolution = _greedySolutionCreator.CreateInitialSolution(nodes);

        var tabuSet = new HashSet<Move>();

        var index = 1;
        while (index < maxIterations)
        {
            var (solutionCandidate, move) = FindSolutionCandidate(bestSolution);

            if (tabuSet.NotContains(move) && solutionCandidate.Cost < bestSolution.Cost)
            {
                bestSolution = solutionCandidate;
                tabuSet.Add(move);
            }

            index++;
        }

        return bestSolution;
    }

    private (Solution solutionCandidate, Move move) FindSolutionCandidate(Solution previousSolution)
    {
        return (previousSolution, new Move());
    }
}