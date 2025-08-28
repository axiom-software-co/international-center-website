using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Medical-grade systems prioritize resilience over specific exception handling in health checks")]
[assembly: SuppressMessage("Performance", "CA1805:Do not initialize unnecessarily", Justification = "Explicit initialization improves code clarity in medical systems")]
[assembly: SuppressMessage("Performance", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Library code uses ConfigureAwait(false) - some instances missed by automation")]
[assembly: SuppressMessage("Performance", "CA1861:Prefer 'static readonly' fields over constant array arguments", Justification = "Health check tag arrays are small and rarely called")]
[assembly: SuppressMessage("Performance", "CA1862:Use StringComparison for case-insensitive string comparisons", Justification = "Database lookups use appropriate comparison methods")]
[assembly: SuppressMessage("Performance", "CA1860:Avoid using 'Enumerable.Any()' extension method", Justification = "Medical audit code prioritizes readability over micro-optimizations")]
[assembly: SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Metrics methods perform operations and are appropriately named as methods")]