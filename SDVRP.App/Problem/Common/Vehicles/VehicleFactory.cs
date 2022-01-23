using System.Collections.Immutable;
using SDVRP.App.Problem.Common.Nodes;

namespace SDVRP.App.Problem.Common.Vehicles;

public class VehicleFactory
{
    private readonly int _vehicleCapacity;
    private int _lastVehicleId = 0;
    
    public VehicleFactory(int vehicleCapacity)
    {
        _vehicleCapacity = vehicleCapacity;
    }

    public Vehicle CreateVehicle(Node startingNode)
        => new()
        {
            Id = ++_lastVehicleId,
            Capacity = _vehicleCapacity,
            Nodes = new List<VisitedNode> { new() {Load = 0, Node = startingNode} }.ToImmutableList()
        };
}
