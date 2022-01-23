using System.Collections.Immutable;
using SDVRP.App.Problem.Common;
using SDVRP.App.Problem.Common.Nodes;
using SDVRP.App.Problem.Common.Vehicles;

namespace SDVRP.App.Problem.Algorithms;

public class GreedySolutionCreator
{
    private readonly VehicleFactory _vehicleFactory;
    private readonly Func<double, double> _costFunction;

    public GreedySolutionCreator(VehicleFactory vehicleFactory, Func<double, double> costFunction)
    {
        _vehicleFactory = vehicleFactory;
        _costFunction = costFunction;
    }
    
    public Solution CreateInitialSolution(IReadOnlyCollection<Node> nodes)
    {
        var startingNode = nodes.Single(x => x.Id == 0);

        var vehicles = new List<Vehicle>();
        
        while (nodes.Sum(x => x.Demand) > 0)
        {
            var (loadedVehicle, nodesList) = GreedyLoad(nodes, startingNode, _vehicleFactory.CreateVehicle(startingNode));
            nodes = nodesList;
            vehicles.Add(loadedVehicle);
        }

        return new Solution(_costFunction)
        {
            Vehicles = vehicles.ToImmutableList()
        };
    }

    private static (Vehicle loadedVehicle, List<Node> nodes) GreedyLoad(
        IReadOnlyCollection<Node> nodes,
        Node startingNode,
        Vehicle vehicleToLoad)
    {
        while (vehicleToLoad.FreeCapacity > 0 && nodes.Sum(x => x.Demand) > 0)
        {
            (vehicleToLoad, nodes) = TryLoadVehicle(nodes, startingNode, vehicleToLoad);
        }
        
        return (vehicleToLoad, nodes.ToList());
    }

    private static (Vehicle loadedVehicle, List<Node> nodes) TryLoadVehicle(
        IReadOnlyCollection<Node> nodes,
        Node previousNode,
        Vehicle vehicleToLoad)
    {
        var nearestNode = nodes
            .WithDemand()
            .ClosestTo(previousNode);

        var (loadedVehicle, loadedNode) = vehicleToLoad.Load(nearestNode);
        
        return (
            loadedVehicle,
            nodes
                .Where(x => x.Id != nearestNode.Id)
                .Append(loadedNode)
                .ToList());
    }
}
