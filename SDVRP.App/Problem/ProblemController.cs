using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using SDVRP.App.Problem.Algorithms;
using SDVRP.App.Problem.Common;
using SDVRP.App.Problem.Common.Vehicles;
using SDVRP.App.Problem.Cost;

namespace SDVRP.App.Problem;

[ApiController]
[Route("[controller]")]
public class ProblemController : ControllerBase
{
    [HttpPost("Solve")]
    public IActionResult SolveProblem([FromBody] Problem problem)
    {
        return problem.IsValid()
            .Bind(SolveProblemWithTabuSearch)
            .Match(
                onSuccess: Ok,
                onFailure: error => (IActionResult)BadRequest(error));
    }

    private static Result<Solution, ProblemError> SolveProblemWithTabuSearch(Problem problem)
    {
        var costFunction = problem.CostFunctionType.GenerateCostFunction();
        var vehicleFactory = new VehicleFactory(problem.VehicleCapacity);
        var greedySolutionCreator = new GreedySolutionCreator(vehicleFactory, costFunction);
        
        var tabuSearch = new TabuSearch(vehicleFactory, greedySolutionCreator);

        return tabuSearch.Solve(problem.Nodes, problem.MaxIterations);
    }
}