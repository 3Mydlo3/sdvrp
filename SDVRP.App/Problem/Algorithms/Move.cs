namespace SDVRP.App.Problem.Algorithms;

internal record Move
{
    public int VehicleId { get; init; }
    
    public int NodeId { get; init; }
}
