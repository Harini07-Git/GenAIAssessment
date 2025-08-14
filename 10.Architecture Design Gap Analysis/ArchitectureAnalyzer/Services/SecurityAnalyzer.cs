using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArchitectureAnalyzer.Services;

public class SecurityAnalyzer : ISecurityAnalyzer
{
    public async Task<List<SecurityIssue>> AnalyzeSecurityAsync(string solutionPath)
    {
        var issues = new List<SecurityIssue>();
        var solutionFolder = Path.GetDirectoryName(solutionPath)!;
        var codeFiles = Directory.GetFiles(solutionFolder, "*.cs", SearchOption.AllDirectories);

        foreach (var file in codeFiles)
        {
            var fileIssues = await AnalyzeFileAsync(file);
            issues.AddRange(fileIssues);
        }

        return issues;
    }

    private async Task<List<SecurityIssue>> AnalyzeFileAsync(string filePath)
    {
        var issues = new List<SecurityIssue>();
        var code = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();

        // Check for hardcoded secrets
        CheckHardcodedSecrets(root, filePath, issues);

        // Check for SQL injection vulnerabilities
        CheckSqlInjection(root, filePath, issues);

        // Check for missing authentication/authorization
        CheckMissingAuthentication(root, filePath, issues);

        return issues;
    }

    private void CheckHardcodedSecrets(SyntaxNode root, string filePath, List<SecurityIssue> issues)
    {
        var stringLiterals = root.DescendantNodes()
            .OfType<LiteralExpressionSyntax>()
            .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression));

        foreach (var literal in stringLiterals)
        {
            if (IsLikelySecret(literal.Token.ValueText))
            {
                issues.Add(new SecurityIssue
                {
                    Title = "Hardcoded Secret Detected",
                    Description = "Potential hardcoded secret or credential found in code",
                    Severity = Severity.High,
                    Location = $"{filePath}:{literal.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    VulnerabilityType = "Sensitive Data Exposure",
                    Impact = "Could lead to unauthorized access if credentials are compromised",
                    Recommendation = "Move secrets to secure configuration or use a secret management service"
                });
            }
        }
    }

    private void CheckSqlInjection(SyntaxNode root, string filePath, List<SecurityIssue> issues)
    {
        var stringConcats = root.DescendantNodes()
            .OfType<BinaryExpressionSyntax>()
            .Where(b => b.IsKind(SyntaxKind.AddExpression));

        foreach (var concat in stringConcats)
        {
            if (IsPotentialSqlInjection(concat))
            {
                issues.Add(new SecurityIssue
                {
                    Title = "Potential SQL Injection",
                    Description = "String concatenation used in potential SQL query",
                    Severity = Severity.High,
                    Location = $"{filePath}:{concat.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    VulnerabilityType = "SQL Injection",
                    Impact = "Could allow unauthorized database access or manipulation",
                    Recommendation = "Use parameterized queries or an ORM"
                });
            }
        }
    }

    private void CheckMissingAuthentication(SyntaxNode root, string filePath, List<SecurityIssue> issues)
    {
        var controllers = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(c => c.Identifier.Text.EndsWith("Controller"));

        foreach (var controller in controllers)
        {
            if (!HasAuthenticationAttribute(controller))
            {
                issues.Add(new SecurityIssue
                {
                    Title = "Missing Authentication",
                    Description = $"Controller '{controller.Identifier.Text}' lacks authentication attributes",
                    Severity = Severity.High,
                    Location = $"{filePath}:{controller.GetLocation().GetLineSpan().StartLinePosition.Line + 1}",
                    VulnerabilityType = "Authentication",
                    Impact = "Endpoints may be accessible without authentication",
                    Recommendation = "Add appropriate authentication attributes"
                });
            }
        }
    }

    private bool IsLikelySecret(string value)
    {
        var secretPatterns = new[]
        {
            "password",
            "secret",
            "key",
            "token",
            "apikey",
            "connectionstring"
        };

        return secretPatterns.Any(pattern => 
            value.ToLowerInvariant().Contains(pattern));
    }

    private bool IsPotentialSqlInjection(BinaryExpressionSyntax node)
    {
        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE" };
        var nodeText = node.ToString().ToUpperInvariant();
        
        return sqlKeywords.Any(keyword => nodeText.Contains(keyword)) &&
               nodeText.Contains("+") &&
               !nodeText.Contains("@");
    }

    private bool HasAuthenticationAttribute(ClassDeclarationSyntax node)
    {
        return node.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attr => attr.Name.ToString().Contains("Authorize") ||
                        attr.Name.ToString().Contains("Authentication"));
    }
}
