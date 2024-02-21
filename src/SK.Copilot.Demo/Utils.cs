using System.Diagnostics.CodeAnalysis;

namespace Demo;

public static class DemoUtils
{
    public static IDemo GetDemo(string demo)
    {
        var types = typeof(DemoUtils).Assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(IDemo))).ToDictionary(t => t.Name, new IgnoreCaseEqualityComparer());
        var selectedType = types[demo];

        return ((IDemo)Activator.CreateInstance(selectedType)!);
    }
}

file class IgnoreCaseEqualityComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
        => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

    public int GetHashCode([DisallowNull] string obj)
        => obj?.ToString()?.ToLower().GetHashCode() ?? -666;
}
