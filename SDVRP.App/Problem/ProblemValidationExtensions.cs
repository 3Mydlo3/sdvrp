using CSharpFunctionalExtensions;
using SDVRP.App.Problem.Common.Nodes;
using SDVRP.App.Problem.Cost;
using SDVRP.App.Problem.Extensions;

namespace SDVRP.App.Problem;

public static class ProblemValidationExtensions
{
    public static Result<Problem, ProblemError> IsValid(this Problem problem)
        => problem.HasZeroDemandStartNode()
                .Bind(HasPositiveIterations)
                .Bind(HasPositiveVehicleCapacity)
                .Bind(CostFunctionIsDefined);

    private static Result<Problem, ProblemError> HasZeroDemandStartNode(this Problem problem)
        => problem.StartNodeExists()
            .Bind(NodeHasZeroDemand)
            .Map(_ => problem);

    private static Result<Node, ProblemError> StartNodeExists(this Problem problem)
        => problem.GetStartNode() is { } node
            ? node
            : new ProblemError("Not found start node with ID 0.");

    private static Node? GetStartNode(this Problem problem)
        => problem.Nodes.SingleOrDefault(x => x.Id == 0);

    private static Result<Node, ProblemError> NodeHasZeroDemand(this Node node)
        => node.Demand == 0 && node.InitialDemand == 0
            ? node
            : new ProblemError("Start node must have 0 demand.");

    private static Result<Problem, ProblemError> HasPositiveIterations(this Problem problem)
        => problem.MaxIterations > 0 
            ? problem
            : new ProblemError($"{nameof(Problem.MaxIterations)} must be greater than 0.");

    private static Result<Problem, ProblemError> HasPositiveVehicleCapacity(this Problem problem)
        => problem.VehicleCapacity > 0
            ? problem
            : new ProblemError($"{nameof(Problem.VehicleCapacity)} must be greater than 0.");

    private static Result<Problem, ProblemError> CostFunctionIsDefined(this Problem problem)
        => Enum.IsDefined(problem.CostFunctionType)
            ? problem
            : new ProblemError(
                $"Unknown {nameof(Problem.CostFunctionType)}. Please use {EnumExtensions.ListAllValues<CostFunctionType>()}.");
}
