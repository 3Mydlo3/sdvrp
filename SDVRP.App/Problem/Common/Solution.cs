using System.Collections.Immutable;
using SDVRP.App.Problem.Common.Vehicles;

namespace SDVRP.App.Problem.Common;

public record Solution(Func<double, double> _costFunction)
{
    private readonly Func<double, double> _costFunction = _costFunction;

    public double Cost => Vehicles.Sum(vehicle => vehicle.CalculateCost(_costFunction));

    public ImmutableList<Vehicle> Vehicles { get; init; } = ImmutableList<Vehicle>.Empty;
}