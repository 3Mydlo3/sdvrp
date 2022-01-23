namespace SDVRP.App.Problem.Common.Nodes;

public record Node : Position
{
    public int Id { get; init; }
    
    public int InitialDemand { get; init; }
    
    public int Demand { get; init; }
}