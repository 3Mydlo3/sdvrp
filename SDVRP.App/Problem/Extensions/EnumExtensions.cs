namespace SDVRP.App.Problem.Extensions;

internal static class EnumExtensions
{
    public static string ListAllValues<T>() where T : struct, Enum => string.Join(", ", Enum.GetNames<T>());
}
