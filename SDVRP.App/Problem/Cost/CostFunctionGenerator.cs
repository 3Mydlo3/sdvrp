namespace SDVRP.App.Problem.Cost;

public static class CostFunctionGenerator
{
    public static Func<double, double> GenerateCostFunction(this CostFunctionType costFunctionType)
    {
        return costFunctionType switch
        {
            CostFunctionType.Linear => x => x,
            CostFunctionType.Quadriatic => x => x * x,
            CostFunctionType.Reversed => x => x * 0.5,
            _ => throw new ArgumentOutOfRangeException(
                nameof(costFunctionType),
                costFunctionType,
                null)
        };
    }
}
