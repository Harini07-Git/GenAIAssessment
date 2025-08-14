using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;

namespace ArchitectureAnalyzer.Services;

public class CodeAnalyzer : ICodeAnalyzer
{
    public async Task<ArchitectureSummary> AnalyzeArchitectureAsync(string solutionPath)
    {
        var summary = new ArchitectureSummary();
        var solutionFolder = Path.GetDirectoryName(solutionPath)!;
        
        // Analyze project files
        var projectFiles = Directory.GetFiles(solutionFolder, "*.csproj", SearchOption.AllDirectories);
        foreach (var projectFile in projectFiles)
        {
            var projectXml = XDocument.Load(projectFile);
            AnalyzeProject(projectXml, summary);
        }

        // Analyze code files
        var codeFiles = Directory.GetFiles(solutionFolder, "*.cs", SearchOption.AllDirectories);
        foreach (var codeFile in codeFiles)
        {
            await AnalyzeCodeFileAsync(codeFile, summary);
        }

        return summary;
    }

    private void AnalyzeProject(XDocument projectXml, ArchitectureSummary summary)
    {
        var packageRefs = projectXml.Descendants("PackageReference");
        foreach (var package in packageRefs)
        {
            var dependency = $"{package.Attribute("Include")?.Value} ({package.Attribute("Version")?.Value})";
            summary.Dependencies.Add(dependency);
        }

        var targetFramework = projectXml.Descendants("TargetFramework").FirstOrDefault()?.Value;
        if (!string.IsNullOrEmpty(targetFramework))
        {
            summary.Technologies.Add($"Target Framework: {targetFramework}");
        }
    }

    private async Task AnalyzeCodeFileAsync(string filePath, ArchitectureSummary summary)
    {
        var code = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();

        // Analyze classes and patterns
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classNode in classes)
        {
            AnalyzeClass(classNode, summary);
        }
    }

    private void AnalyzeClass(ClassDeclarationSyntax classNode, ArchitectureSummary summary)
    {
        // Detect common patterns
        if (classNode.Identifier.Text.EndsWith("Controller"))
        {
            if (!summary.MainComponents.Contains("MVC/API Controllers"))
                summary.MainComponents.Add("MVC/API Controllers");
        }
        else if (classNode.Identifier.Text.EndsWith("Service"))
        {
            if (!summary.MainComponents.Contains("Service Layer"))
                summary.MainComponents.Add("Service Layer");
        }
        else if (classNode.Identifier.Text.EndsWith("Repository"))
        {
            if (!summary.MainComponents.Contains("Repository Layer"))
                summary.MainComponents.Add("Repository Layer");
        }

        // Detect architectural patterns
        if (summary.MainComponents.Contains("Repository Layer") && 
            summary.MainComponents.Contains("Service Layer"))
        {
            if (!summary.CurrentArchitecturePattern.Contains("Layered Architecture"))
                summary.CurrentArchitecturePattern = "Layered Architecture";
        }
    }
}
