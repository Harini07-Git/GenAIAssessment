using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;

namespace ArchitectureAnalyzer.Services;

public class ModernizationAnalyzer : IModernizationAnalyzer
{
    public async Task<List<ModernizationRecommendation>> AnalyzeModernizationOpportunitiesAsync(string solutionPath)
    {
        var recommendations = new List<ModernizationRecommendation>();
        var solutionFolder = Path.GetDirectoryName(solutionPath)!;

        // Analyze project files for framework versions and dependencies
        var projectFiles = Directory.GetFiles(solutionFolder, "*.csproj", SearchOption.AllDirectories);
        foreach (var projectFile in projectFiles)
        {
            AnalyzeProjectFile(projectFile, recommendations);
        }

        // Analyze code files for modernization opportunities
        var codeFiles = Directory.GetFiles(solutionFolder, "*.cs", SearchOption.AllDirectories);
        foreach (var file in codeFiles)
        {
            var fileRecommendations = await AnalyzeFileAsync(file);
            recommendations.AddRange(fileRecommendations);
        }

        return recommendations;
    }

    private void AnalyzeProjectFile(string projectPath, List<ModernizationRecommendation> recommendations)
    {
        var projectXml = XDocument.Load(projectPath);
        var targetFramework = projectXml.Descendants("TargetFramework").FirstOrDefault()?.Value;

        if (!string.IsNullOrEmpty(targetFramework))
        {
            if (targetFramework.Contains("netcoreapp") || targetFramework.Contains("net5.0"))
            {
                recommendations.Add(new ModernizationRecommendation
                {
                    Title = "Framework Upgrade Opportunity",
                    Description = $"Project is using {targetFramework}. Consider upgrading to .NET 7 or later",
                    Severity = Severity.Medium,
                    Location = projectPath,
                    Category = "Framework",
                    BenefitDescription = "Access to latest performance improvements, features, and security updates",
                    Prerequisites = new List<string> { "Review breaking changes", "Update dependencies", "Test application thoroughly" }
                });
            }
        }

        // Check for older package references
        var packageRefs = projectXml.Descendants("PackageReference");
        foreach (var package in packageRefs)
        {
            AnalyzePackageReference(package, recommendations, projectPath);
        }
    }

    private void AnalyzePackageReference(XElement package, List<ModernizationRecommendation> recommendations, string projectPath)
    {
        var packageId = package.Attribute("Include")?.Value;
        var version = package.Attribute("Version")?.Value;

        if (string.IsNullOrEmpty(packageId) || string.IsNullOrEmpty(version)) return;

        // Check for packages with known modern alternatives
        var modernAlternatives = new Dictionary<string, string>
        {
            { "Newtonsoft.Json", "System.Text.Json" },
            { "Log4Net", "Microsoft.Extensions.Logging" },
            { "EntityFramework", "Microsoft.EntityFrameworkCore" },
            { "WebApi", "Microsoft.AspNetCore.Mvc" }
        };

        if (modernAlternatives.TryGetValue(packageId, out var alternative))
        {
            recommendations.Add(new ModernizationRecommendation
            {
                Title = "Package Modernization",
                Description = $"Consider replacing {packageId} with {alternative}",
                Severity = Severity.Medium,
                Location = projectPath,
                Category = "Dependencies",
                BenefitDescription = "Better performance, modern features, and maintained packages",
                Prerequisites = new List<string> { "Review API changes", "Update dependent code", "Test functionality" }
            });
        }
    }

    private async Task<List<ModernizationRecommendation>> AnalyzeFileAsync(string filePath)
    {
        var recommendations = new List<ModernizationRecommendation>();
        var code = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();

        // Check for modernization opportunities in the code
        CheckForAsyncOpportunities(root, filePath, recommendations);
        CheckForContainerization(root, filePath, recommendations);
        CheckForMicroservicesOpportunities(root, filePath, recommendations);
        CheckForCloudPatterns(root, filePath, recommendations);

        return recommendations;
    }

    private void CheckForAsyncOpportunities(SyntaxNode root, string filePath, List<ModernizationRecommendation> recommendations)
    {
        var syncMethods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => !m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.AsyncKeyword)) &&
                       HasIOOperations(m));

        foreach (var method in syncMethods)
        {
            recommendations.Add(new ModernizationRecommendation
            {
                Title = "Async/Await Opportunity",
                Description = $"Method '{method.Identifier.Text}' could benefit from async/await pattern",
                Severity = Severity.Medium,
                Location = $"{filePath}:{method.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                Category = "Performance",
                BenefitDescription = "Improved scalability and resource utilization",
                Prerequisites = new List<string>
                {
                    "Review method call chain",
                    "Update method signature",
                    "Implement async operations",
                    "Test concurrent scenarios"
                }
            });
        }
    }

    private void CheckForContainerization(SyntaxNode root, string filePath, List<ModernizationRecommendation> recommendations)
    {
        var configurationAccess = root.DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .Any(m => m.ToString().Contains("ConfigurationManager") ||
                     m.ToString().Contains("AppSettings"));

        if (configurationAccess)
        {
            recommendations.Add(new ModernizationRecommendation
            {
                Title = "Containerization Opportunity",
                Description = "Application uses traditional configuration. Consider containerization",
                Severity = Severity.Medium,
                Location = filePath,
                Category = "Cloud-Native",
                BenefitDescription = "Better deployment consistency, scalability, and isolation",
                Prerequisites = new List<string>
                {
                    "Create Dockerfile",
                    "Implement environment-based configuration",
                    "Update deployment scripts",
                    "Set up container orchestration"
                }
            });
        }
    }

    private void CheckForMicroservicesOpportunities(SyntaxNode root, string filePath, List<ModernizationRecommendation> recommendations)
    {
        var classes = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>();

        foreach (var cls in classes)
        {
            if (IsLargeClass(cls) && HasMultipleConcerns(cls))
            {
                recommendations.Add(new ModernizationRecommendation
                {
                    Title = "Microservices Candidate",
                    Description = $"Class '{cls.Identifier.Text}' could be split into microservices",
                    Severity = Severity.Medium,
                    Location = $"{filePath}:{cls.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    Category = "Architecture",
                    BenefitDescription = "Better scalability, maintainability, and team autonomy",
                    Prerequisites = new List<string>
                    {
                        "Identify bounded contexts",
                        "Design service interfaces",
                        "Plan data separation",
                        "Implement service communication"
                    }
                });
            }
        }
    }

    private void CheckForCloudPatterns(SyntaxNode root, string filePath, List<ModernizationRecommendation> recommendations)
    {
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            if (IsLongRunningOperation(method))
            {
                recommendations.Add(new ModernizationRecommendation
                {
                    Title = "Serverless Opportunity",
                    Description = $"Method '{method.Identifier.Text}' could be implemented as a serverless function",
                    Severity = Severity.Medium,
                    Location = $"{filePath}:{method.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    Category = "Cloud-Native",
                    BenefitDescription = "Cost-effective, scalable, and maintenance-free infrastructure",
                    Prerequisites = new List<string>
                    {
                        "Extract function logic",
                        "Implement cloud function",
                        "Set up triggers",
                        "Configure monitoring"
                    }
                });
            }
        }
    }

    private bool HasIOOperations(MethodDeclarationSyntax method)
    {
        if (method.Body == null) return false;

        return method.Body.ToString().Contains("File.") ||
               method.Body.ToString().Contains("Stream") ||
               method.Body.ToString().Contains("Http") ||
               method.Body.ToString().Contains("Sql");
    }

    private bool IsLargeClass(ClassDeclarationSyntax cls)
    {
        var methodCount = cls.Members.Count(m => m is MethodDeclarationSyntax);
        var propertyCount = cls.Members.Count(m => m is PropertyDeclarationSyntax);

        return methodCount > 10 || propertyCount > 15;
    }

    private bool HasMultipleConcerns(ClassDeclarationSyntax cls)
    {
        var hasDataAccess = cls.ToString().Contains("DbContext") || 
                           cls.ToString().Contains("Repository");
        var hasBusinessLogic = cls.ToString().Contains("Service") || 
                              cls.ToString().Contains("Manager");
        var hasPresentation = cls.ToString().Contains("Controller") || 
                             cls.ToString().Contains("View");

        return (hasDataAccess && hasBusinessLogic) ||
               (hasBusinessLogic && hasPresentation) ||
               (hasDataAccess && hasPresentation);
    }

    private bool IsLongRunningOperation(MethodDeclarationSyntax method)
    {
        if (method.Body == null) return false;

        return method.Body.ToString().Contains("Thread.Sleep") ||
               method.Body.ToString().Contains("Task.Delay") ||
               method.Body.ToString().Contains("while") ||
               method.Body.ToString().Contains("for");
    }
}
