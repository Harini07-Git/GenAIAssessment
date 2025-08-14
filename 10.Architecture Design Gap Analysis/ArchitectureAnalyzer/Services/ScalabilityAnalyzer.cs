using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArchitectureAnalyzer.Services;

public class ScalabilityAnalyzer : IScalabilityAnalyzer
{
    public async Task<List<ScalabilityIssue>> AnalyzeScalabilityAsync(string solutionPath)
    {
        var issues = new List<ScalabilityIssue>();
        var solutionFolder = Path.GetDirectoryName(solutionPath)!;
        var codeFiles = Directory.GetFiles(solutionFolder, "*.cs", SearchOption.AllDirectories);

        foreach (var file in codeFiles)
        {
            var fileIssues = await AnalyzeFileAsync(file);
            issues.AddRange(fileIssues);
        }

        return issues;
    }

    private async Task<List<ScalabilityIssue>> AnalyzeFileAsync(string filePath)
    {
        var issues = new List<ScalabilityIssue>();
        var code = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();

        // Check for synchronous I/O operations
        CheckSynchronousIO(root, filePath, issues);

        // Check for missing caching
        CheckMissingCaching(root, filePath, issues);

        // Check for potential N+1 queries
        CheckNPlusOneQueries(root, filePath, issues);

        return issues;
    }

    private void CheckSynchronousIO(SyntaxNode root, string filePath, List<ScalabilityIssue> issues)
    {
        var syncMethods = new[]
        {
            "File.ReadAllText",
            "File.WriteAllText",
            "File.ReadAllBytes",
            "File.WriteAllBytes",
            "Stream.Read",
            "Stream.Write"
        };

        var invocations = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var methodName = invocation.ToString();
            if (syncMethods.Any(sm => methodName.Contains(sm)))
            {
                issues.Add(new ScalabilityIssue
                {
                    Title = "Synchronous I/O Operation",
                    Description = $"Synchronous I/O operation detected: {methodName}",
                    Severity = Severity.High,
                    Location = $"{filePath}:{invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    BottleneckType = "I/O Operations",
                    PerformanceImpact = "Blocks thread pool threads and reduces application throughput",
                    Recommendation = "Use async/await alternatives like ReadAllTextAsync"
                });
            }
        }
    }

    private void CheckMissingCaching(SyntaxNode root, string filePath, List<ScalabilityIssue> issues)
    {
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            if (IsExpensiveOperation(method) && !HasCaching(method))
            {
                issues.Add(new ScalabilityIssue
                {
                    Title = "Missing Cache Implementation",
                    Description = $"Potentially expensive operation without caching in method {method.Identifier.Text}",
                    Severity = Severity.Medium,
                    Location = $"{filePath}:{method.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    BottleneckType = "Performance",
                    PerformanceImpact = "May cause unnecessary load on resources",
                    Recommendation = "Consider implementing caching for expensive operations"
                });
            }
        }
    }

    private void CheckNPlusOneQueries(SyntaxNode root, string filePath, List<ScalabilityIssue> issues)
    {
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            if (HasPotentialNPlusOneQuery(method))
            {
                issues.Add(new ScalabilityIssue
                {
                    Title = "Potential N+1 Query",
                    Description = $"Possible N+1 query pattern detected in method {method.Identifier.Text}",
                    Severity = Severity.High,
                    Location = $"{filePath}:{method.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    BottleneckType = "Database Access",
                    PerformanceImpact = "Multiple database roundtrips that could be consolidated",
                    Recommendation = "Use Include() or eager loading to fetch related data in a single query"
                });
            }
        }
    }

    private bool IsExpensiveOperation(MethodDeclarationSyntax method)
    {
        var methodBody = method.Body?.ToString() ?? "";
        
        return methodBody.Contains("Database") ||
               methodBody.Contains("Http") ||
               methodBody.Contains("File") ||
               methodBody.Contains("Stream") ||
               method.AttributeLists.Any(a => a.ToString().Contains("HttpGet"));
    }

    private bool HasCaching(MethodDeclarationSyntax method)
    {
        var methodBody = method.Body?.ToString() ?? "";
        
        return methodBody.Contains("Cache") ||
               methodBody.Contains("MemoryCache") ||
               methodBody.Contains("IDistributedCache") ||
               method.AttributeLists.Any(a => a.ToString().Contains("Cache"));
    }

    private bool HasPotentialNPlusOneQuery(MethodDeclarationSyntax method)
    {
        if (method.Body == null) return false;

        var hasForEach = method.Body.DescendantNodes()
            .OfType<ForEachStatementSyntax>()
            .Any();

        var hasDbContext = method.Body.ToString()
            .Contains("DbContext") || method.Body.ToString()
            .Contains("Repository");

        return hasForEach && hasDbContext;
    }
}
