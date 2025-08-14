using ArchitectureAnalyzer.Core;
using ArchitectureAnalyzer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArchitectureAnalyzer;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide the solution path to analyze.");
            return;
        }

        var solutionPath = args[0];
        if (!File.Exists(solutionPath))
        {
            Console.WriteLine($"Solution file not found at: {solutionPath}");
            return;
        }

        var services = ConfigureServices();
        var analyzer = services.GetRequiredService<IArchitectureAnalyzer>();
        
        Console.WriteLine("Starting architecture analysis...");
        var report = await analyzer.AnalyzeAsync(solutionPath);
        
        var reportGenerator = services.GetRequiredService<IReportGenerator>();
        await reportGenerator.GenerateReportAsync(report, "architecture-analysis-report");
        
        Console.WriteLine("Analysis complete. Reports have been generated.");
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register core services
        services.AddScoped<IArchitectureAnalyzer, ArchitectureAnalyzer>();
        services.AddScoped<IReportGenerator, ReportGenerator>();
        
        // Register analyzers
        services.AddScoped<ICodeAnalyzer, CodeAnalyzer>();
        services.AddScoped<ISecurityAnalyzer, SecurityAnalyzer>();
        services.AddScoped<IScalabilityAnalyzer, ScalabilityAnalyzer>();
        services.AddScoped<ITechnicalDebtAnalyzer, TechnicalDebtAnalyzer>();
        services.AddScoped<IModernizationAnalyzer, ModernizationAnalyzer>();

        return services.BuildServiceProvider();
    }
}
