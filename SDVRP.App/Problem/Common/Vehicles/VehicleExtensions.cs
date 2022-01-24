using SDVRP.App.Problem.Common.Nodes;

namespace SDVRP.App.Problem.Common.Vehicles;

public static class VehicleExtensions
{
    public static IEnumerable<Vehicle> GetVehiclesWithFreeCapacity(this IEnumerable<Vehicle> vehicles)
        => vehicles.Where(x => x.FreeCapacity > 0);

    public static int FreeCapacity(this IEnumerable<Vehicle> vehicles) => vehicles.Sum(x => x.FreeCapacity);
    
    public static VisitedNode? GetNode(this Vehicle vehicle, int nodeId)
        => vehicle.Path.SingleOrDefault(x => x.Node.Id == nodeId);
    
    public static IEnumerable<Vehicle> DeliveringTo(this IEnumerable<Vehicle> vehicles, VisitedNode node)
        => vehicles.DeliveringTo(node.Node.Id);

    public static IEnumerable<Vehicle> DeliveringTo(this IEnumerable<Vehicle> vehicles, Node node)
        => vehicles.DeliveringTo(node.Id);
    
    public static IEnumerable<Vehicle> DeliveringTo(this IEnumerable<Vehicle> vehicles, int nodeId)
        => vehicles.Where(x => x.Path.Any(visitedNode => visitedNode.Node.Id == nodeId && visitedNode.Load > 0));

    public static int SumDeliveryTo(this IEnumerable<Vehicle> vehicles, int nodeId)
        => vehicles
            .DeliveringTo(nodeId)
            .Sum(x => x.Path.Single(x => x.Node.Id == nodeId).Load);
}
