using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using SDVRP.App.Problem;
using SDVRP.App.Problem.Algorithms;
using SDVRP.App.Problem.Common;
using SDVRP.App.Problem.Common.Nodes;
using SDVRP.App.Problem.Common.Vehicles;
using Xunit;

namespace SDVRP.App.Tests.Problem;

public class InitialSolutionCreatorTests
{
    [Fact]
    public void CreateInitialSolution_Created_Correct()
    {
        double CostFunction(double x) => x * 2;
        const int vehicleCapacity = 25;

        var initialSolutionCreator = new GreedySolutionCreator(new VehicleFactory(vehicleCapacity), CostFunction);
        
        var nodes = new List<Node>
        {
            new()
            {
                Id = 0,
                X = 0,
                Y = 100,
                InitialDemand = 0,
                Demand = 0
            },
            new()
            {
                Id = 1,
                X = 100,
                Y = 100,
                InitialDemand = 20,
                Demand = 20
            },
            new()
            {
                Id = 2,
                X = 200,
                Y = 100,
                InitialDemand = 30,
                Demand = 30
            }
        };
        
        var expectedSolution = new Solution(CostFunction)
        {
            Vehicles = new List<Vehicle>
            {
                new()
                {
                    Id = 1,
                    Capacity = vehicleCapacity,
                    Nodes = new List<VisitedNode>
                    {
                        new() { Load = 0, Node = nodes.ElementAt(0)},
                        new() { Load = 25, Node = nodes.ElementAt(1)},
                        new() { Load = 5, Node = nodes.ElementAt(2)}
                    }.ToImmutableList()
                },
                new()
                {
                    Id = 2,
                    Capacity = vehicleCapacity,
                    Nodes = new List<VisitedNode>
                    {
                        new() { Load = 0, Node = nodes.ElementAt(0)},
                        new() { Load = 25, Node = nodes.ElementAt(2) with {Demand = 25}}
                    }.ToImmutableList()
                }
            }.ToImmutableList()
        };
        
        var initialSolution = initialSolutionCreator.CreateInitialSolution(nodes);

        initialSolution.Should().BeEquivalentTo(expectedSolution);
    }
}
