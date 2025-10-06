using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Foundatio;

public static class FoundatioDiagnostics
{
    public static readonly ActivitySource ActivitySource = new("Foundatio", "");
    public static readonly Meter Meter = new("Foundatio", "");
}
