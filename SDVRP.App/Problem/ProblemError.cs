namespace SDVRP.App.Problem;

public class ProblemError
{
    public ProblemError(string notFoundStartNodeWithId)
    {
        Message = notFoundStartNodeWithId;
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string Message { get; }
}
