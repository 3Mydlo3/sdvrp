using System.Collections.Immutable;
using CSharpFunctionalExtensions;
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

        var tabuList = new TabuList<Move>(50);

        var index = 1;
        var newSolutionFindResult = Result.Success();
        while (index < maxIterations && newSolutionFindResult.IsSuccess)
        {
            newSolutionFindResult = FindSolutionCandidate(bestSolution)
                .OrderByDescending(candidate => candidate.solutionCandidate.Cost)
                .TryFirst(candidate => tabuList.NotContains(candidate.move))
                .Bind(candidate => CompareWithBestSolution(bestSolution, candidate))
                .Match(
                    Some: candidate =>
                    {
                        bestSolution = candidate.solutionCandidate;
                        tabuList.Add(candidate.move);
                        return Result.Success();
                    },
                    None: () => tabuList.Any()
                        ? tabuList.Clear()
                        : Result.Failure("Could not find any more good candidates."));

            index++;
        }

        return bestSolution;
    }

    private static Maybe<(Solution solutionCandidate, Move move)> CompareWithBestSolution(
        Solution bestSolution,
        (Solution solutionCandidate, Move move) solutionCandidateAndMove)
    {
        return bestSolution.Cost > solutionCandidateAndMove.solutionCandidate.Cost
            ? solutionCandidateAndMove
            : Maybe.None;
    }

    private IEnumerable<(Solution solutionCandidate, Move move)> FindSolutionCandidate(Solution previousSolution)
    {
        for (var vehicleIndex = 0; vehicleIndex < previousSolution.Vehicles.Count; vehicleIndex++)
        {
            var vehicle = previousSolution.Vehicles[vehicleIndex];
            foreach (var solutionNode in previousSolution.Nodes
                         .Where(solutionNode => vehicle.Path
                             .All(vehicleNode => vehicleNode.Node.Id != solutionNode.Id)))
            {
                for (var vehicleNodeIndex = 0; vehicleNodeIndex < vehicle.Path.Count; vehicleNodeIndex++)
                {
                    var newNode = new VisitedNode
                    {
                        Load = solutionNode.Demand,
                        Node = solutionNode
                    };
                    
                    var vehicleWithEditedPath = AddNodeAtIndex(vehicle, vehicleNodeIndex, newNode);

                    var vehicles = AdjustNodeLoadForOtherVehicles(newNode, previousSolution.Vehicles, vehicleWithEditedPath);
                    
                    vehicles.RemoveAt(vehicleIndex);
                    vehicles.Insert(vehicleIndex, vehicleWithEditedPath);

                    var newVehicles = CoverAnyRemainingDemand(previousSolution.Nodes, vehicles);
                    
                    yield return (previousSolution with
                    {
                        Vehicles = newVehicles.ToImmutableList()
                    }, new Move
                    {
                        NodeId = solutionNode.Id,
                        VehicleId = vehicle.Id
                    });
                }
            }
        }
    }

    private List<Vehicle> CoverAnyRemainingDemand(IReadOnlyCollection<Node> nodes, IReadOnlyList<Vehicle> vehicles)
    {
        foreach (var node in nodes)
        {
            while (vehicles.SumDeliveryTo(node.Id) < node.Demand)
            {
                if (vehicles.FreeCapacity() == 0)
                {
                    vehicles = vehicles.Append(_vehicleFactory.CreateVehicle(nodes.StartingNode()))
                        .ToList();
                }

                var (loadedVehicle, _) = vehicles
                    .GetVehiclesWithFreeCapacity()
                    .First()
                    .Load(node);

                vehicles = vehicles
                    .Where(x => x.Id != loadedVehicle.Id)
                    .Append(loadedVehicle)
                    .ToList();
            }
        }

        return vehicles.ToList();
    }

    private static List<Vehicle> AdjustNodeLoadForOtherVehicles(VisitedNode editedNode, IReadOnlyList<Vehicle> previousSolutionVehicles, Vehicle vehicleWithEditedPath)
    {
        var vehiclesDeliveringToEditedNode = previousSolutionVehicles
            .Where(x => x.Id != vehicleWithEditedPath.Id)
            .DeliveringTo(editedNode)
            .ToList();

        var remainingDemand = editedNode.Node.InitialDemand - editedNode.Load - vehiclesDeliveringToEditedNode.Sum(
            x => x.Path.Single(x => x.Node.Id == editedNode.Node.Id).Load);

        var adjustedVehicles = previousSolutionVehicles
            .ToList();
        
        while (remainingDemand < 0 || !vehiclesDeliveringToEditedNode.Any())
        {
            var vehicleToAdjust = vehiclesDeliveringToEditedNode.First();
            vehiclesDeliveringToEditedNode.RemoveAt(0);
            var (nextVehicle, decreasedLoad) = DecreaseNodeLoad(vehicleToAdjust, editedNode.Node.Id, remainingDemand);

            adjustedVehicles = adjustedVehicles
                .Where(x => x.Id != nextVehicle.Id)
                .Append(nextVehicle)
                .ToList();
            remainingDemand -= decreasedLoad;
        }

        return adjustedVehicles;
    }

    private static (Vehicle vehicle, int decreasedLoad) DecreaseNodeLoad(
        Vehicle vehicle,
        int nodeId,
        int maxLoadDecrease)
    {
        var visitedNode = vehicle.Path.SingleOrDefault(x => x.Node.Id == nodeId);

        if (visitedNode is null) return (vehicle, 0);

        if (visitedNode.Load <= maxLoadDecrease)
            return (RemoveNodeFromVehiclePath(vehicle, nodeId), vehicle.GetNode(nodeId)?.Load ?? 0);
        
        var updatedNode = visitedNode with
        {
            Load = visitedNode.Load - maxLoadDecrease
        };
        var nodeIndex = vehicle.Path.IndexOf(visitedNode);

        var newPath = vehicle.Path.ToList();
        newPath.RemoveAt(nodeIndex);
        newPath.Insert(nodeIndex, updatedNode);

        return (vehicle with
        {
            Path = newPath.ToImmutableList()
        }, maxLoadDecrease);

    }

    private static Vehicle RemoveNodeFromVehiclePath(Vehicle vehicle, int nodeId)
    {
        return vehicle with
        {
            Path = vehicle.Path
                .Where(x => x.Node.Id != nodeId)
                .ToImmutableList()
        };
    }

    private static Vehicle AddNodeAtIndex(Vehicle vehicle, int position, VisitedNode newNode)
    {
        var vehiclePath = vehicle.Path
            .ToList();
        
        vehiclePath.Insert(position, newNode);

        var newVehicle = vehicle with
        {
            Path = vehiclePath.ToImmutableList()
        };

        while (newVehicle.FreeCapacity < 0)
        {
            var lastNode = vehiclePath.Last();
            
            if (newVehicle.OverloadCapacity > lastNode.Load)
            {
                vehiclePath = vehiclePath
                    .SkipLast(1)
                    .ToList();
            }
            else
            {
                vehiclePath = vehiclePath
                    .SkipLast(1)
                    .Append(lastNode with { Load = -newVehicle.OverloadCapacity})
                    .ToList();
            }
        }

        return newVehicle;
    }
}