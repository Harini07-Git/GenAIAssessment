using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArchitectureAnalyzer.Services;

public class TechnicalDebtAnalyzer : ITechnicalDebtAnalyzer
{
    public async Task<List<TechnicalDebtIssue>> AnalyzeTechnicalDebtAsync(string solutionPath)
    {
        var issues = new List<TechnicalDebtIssue>();
        var solutionFolder = Path.GetDirectoryName(solutionPath)!;
        var codeFiles = Directory.GetFiles(solutionFolder, "*.cs", SearchOption.AllDirectories);

        foreach (var file in codeFiles)
        {
            var fileIssues = await AnalyzeFileAsync(file);
            issues.AddRange(fileIssues);
        }

        return issues;
    }

    private async Task<List<TechnicalDebtIssue>> AnalyzeFileAsync(string filePath)
    {
        var issues = new List<TechnicalDebtIssue>();
        var code = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();

        // Check code complexity
        CheckMethodComplexity(root, filePath, issues);

        // Check for code duplication
        CheckCodeDuplication(root, filePath, issues);

        // Check for outdated patterns
        CheckOutdatedPatterns(root, filePath, issues);

        // Check for missing documentation
        CheckMissingDocumentation(root, filePath, issues);

        return issues;
    }

    private void CheckMethodComplexity(SyntaxNode root, string filePath, List<TechnicalDebtIssue> issues)
    {
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            var complexity = CalculateCyclomaticComplexity(method);
            if (complexity > 10)
            {
                issues.Add(new TechnicalDebtIssue
                {
                    Title = "High Method Complexity",
                    Description = $"Method '{method.Identifier.Text}' has high cyclomatic complexity ({complexity})",
                    Severity = complexity > 15 ? Severity.High : Severity.Medium,
                    Location = $"{filePath}:{method.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    DebtCategory = "Code Complexity",
                    MaintenanceImpact = "Difficult to maintain and test",
                    Recommendation = "Consider breaking down the method into smaller, more focused methods"
                });
            }
        }
    }

    private void CheckCodeDuplication(SyntaxNode root, string filePath, List<TechnicalDebtIssue> issues)
    {
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .ToList();

        for (int i = 0; i < methods.Count; i++)
        {
            for (int j = i + 1; j < methods.Count; j++)
            {
                if (AreSimilarMethods(methods[i], methods[j]))
                {
                    issues.Add(new TechnicalDebtIssue
                    {
                        Title = "Potential Code Duplication",
                        Description = $"Methods '{methods[i].Identifier.Text}' and '{methods[j].Identifier.Text}' appear to be similar",
                        Severity = Severity.Medium,
                        Location = $"{filePath}:{methods[i].GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                        DebtCategory = "Code Duplication",
                        MaintenanceImpact = "Increases maintenance effort and risk of inconsistent updates",
                        Recommendation = "Consider extracting common functionality into a shared method"
                    });
                }
            }
        }
    }

    private void CheckOutdatedPatterns(SyntaxNode root, string filePath, List<TechnicalDebtIssue> issues)
    {
        var classes = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>();

        foreach (var cls in classes)
        {
            if (UsesOutdatedPatterns(cls))
            {
                issues.Add(new TechnicalDebtIssue
                {
                    Title = "Outdated Design Pattern",
                    Description = $"Class '{cls.Identifier.Text}' uses outdated patterns or practices",
                    Severity = Severity.Medium,
                    Location = $"{filePath}:{cls.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    DebtCategory = "Design Patterns",
                    MaintenanceImpact = "May not follow current best practices",
                    Recommendation = "Consider updating to modern patterns and practices"
                });
            }
        }
    }

    private void CheckMissingDocumentation(SyntaxNode root, string filePath, List<TechnicalDebtIssue> issues)
    {
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            if (!HasDocumentation(method) && IsPublic(method))
            {
                issues.Add(new TechnicalDebtIssue
                {
                    Title = "Missing Documentation",
                    Description = $"Public method '{method.Identifier.Text}' lacks XML documentation",
                    Severity = Severity.Low,
                    Location = $"{filePath}:{method.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    DebtCategory = "Documentation",
                    MaintenanceImpact = "Reduces code maintainability and usability",
                    Recommendation = "Add XML documentation comments explaining the method's purpose, parameters, and return value"
                });
            }
        }
    }

    private int CalculateCyclomaticComplexity(MethodDeclarationSyntax method)
    {
        var complexity = 1; // Base complexity

        complexity += method.DescendantNodes().Count(n =>
            n is IfStatementSyntax ||
            n is ForStatementSyntax ||
            n is ForEachStatementSyntax ||
            n is WhileStatementSyntax ||
            n is DoStatementSyntax ||
            n is CatchClauseSyntax ||
            n is ConditionalExpressionSyntax ||
            n is BinaryExpressionSyntax binary && (
                binary.IsKind(SyntaxKind.LogicalAndExpression) ||
                binary.IsKind(SyntaxKind.LogicalOrExpression)
            )
        );

        return complexity;
    }

    private bool AreSimilarMethods(MethodDeclarationSyntax method1, MethodDeclarationSyntax method2)
    {
        if (method1.Body == null || method2.Body == null) return false;

        var body1 = method1.Body.ToString();
        var body2 = method2.Body.ToString();

        // Simple similarity check based on normalized body content
        var normalized1 = NormalizeCode(body1);
        var normalized2 = NormalizeCode(body2);

        return normalized1 == normalized2;
    }

    private string NormalizeCode(string code)
    {
        return code
            .Replace(" ", "")
            .Replace("\t", "")
            .Replace("\r", "")
            .Replace("\n", "");
    }

    private bool UsesOutdatedPatterns(ClassDeclarationSyntax cls)
    {
        var classContent = cls.ToString();
        
        return classContent.Contains("WebForms") ||
               classContent.Contains("DataSet") ||
               classContent.Contains("DataTable") ||
               (cls.BaseList?.ToString() ?? "").Contains("IDisposable"); // Potential obsolete disposal pattern
    }

    private bool HasDocumentation(MethodDeclarationSyntax method)
    {
        var trivia = method.GetLeadingTrivia();
        return trivia.Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                              t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
    }

    private bool IsPublic(MethodDeclarationSyntax method)
    {
        return method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
    }
}
