using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace ArchitectureAnalyzer.Services;

public class ReportGenerator : IReportGenerator
{
    public async Task GenerateReportAsync(AnalysisReport report, string outputPath)
    {
        await GenerateHtmlReportAsync(report, $"{outputPath}.html");
        await GeneratePdfReportAsync(report, $"{outputPath}.pdf");
    }

    private async Task GenerateHtmlReportAsync(AnalysisReport report, string outputPath)
    {
        var html = new System.Text.StringBuilder();

        // Header
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"UTF-8\">");
        html.AppendLine("<title>Architecture Analysis Report</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
        html.AppendLine(".severity-high { color: #d32f2f; }");
        html.AppendLine(".severity-medium { color: #f57c00; }");
        html.AppendLine(".severity-low { color: #388e3c; }");
        html.AppendLine("h1, h2 { color: #2196f3; }");
        html.AppendLine(".issue { margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 4px; }");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Report Header
        html.AppendLine($"<h1>Architecture Analysis Report</h1>");
        html.AppendLine($"<p>Generated: {report.AnalysisTimestamp}</p>");
        html.AppendLine($"<p>Version: {report.Version}</p>");
        html.AppendLine($"<p>Solution: {report.SolutionPath}</p>");

        // Architecture Summary
        html.AppendLine("<h2>Architecture Summary</h2>");
        html.AppendLine("<div class='section'>");
        html.AppendLine($"<p><strong>Current Architecture Pattern:</strong> {report.ArchitectureSummary.CurrentArchitecturePattern}</p>");
        
        html.AppendLine("<h3>Main Components</h3>");
        html.AppendLine("<ul>");
        foreach (var component in report.ArchitectureSummary.MainComponents)
        {
            html.AppendLine($"<li>{component}</li>");
        }
        html.AppendLine("</ul>");

        html.AppendLine("<h3>Technologies Used</h3>");
        html.AppendLine("<ul>");
        foreach (var tech in report.ArchitectureSummary.Technologies)
        {
            html.AppendLine($"<li>{tech}</li>");
        }
        html.AppendLine("</ul>");
        html.AppendLine("</div>");

        // Security Issues
        html.AppendLine("<h2>Security Issues</h2>");
        foreach (var issue in report.SecurityIssues.OrderByDescending(i => i.Severity))
        {
            AppendIssueHtml(html, issue);
        }

        // Scalability Issues
        html.AppendLine("<h2>Scalability Issues</h2>");
        foreach (var issue in report.ScalabilityIssues.OrderByDescending(i => i.Severity))
        {
            AppendIssueHtml(html, issue);
        }

        // Technical Debt
        html.AppendLine("<h2>Technical Debt</h2>");
        foreach (var issue in report.TechnicalDebtIssues.OrderByDescending(i => i.Severity))
        {
            AppendIssueHtml(html, issue);
        }

        // Modernization Recommendations
        html.AppendLine("<h2>Modernization Recommendations</h2>");
        foreach (var recommendation in report.ModernizationRecommendations.OrderByDescending(r => r.Severity))
        {
            AppendRecommendationHtml(html, recommendation);
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        await File.WriteAllTextAsync(outputPath, html.ToString());
    }

    private async Task GeneratePdfReportAsync(AnalysisReport report, string outputPath)
    {
        using var writer = new PdfWriter(outputPath);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        // Title
        document.Add(new Paragraph("Architecture Analysis Report")
            .SetFontSize(20)
            .SetBold());

        // Header Info
        document.Add(new Paragraph($"Generated: {report.AnalysisTimestamp}"));
        document.Add(new Paragraph($"Version: {report.Version}"));
        document.Add(new Paragraph($"Solution: {report.SolutionPath}"));

        // Architecture Summary
        document.Add(new Paragraph("Architecture Summary")
            .SetFontSize(16)
            .SetBold());

        document.Add(new Paragraph($"Current Architecture Pattern: {report.ArchitectureSummary.CurrentArchitecturePattern}"));

        // Main Components
        document.Add(new Paragraph("Main Components")
            .SetFontSize(14)
            .SetBold());
        foreach (var component in report.ArchitectureSummary.MainComponents)
        {
            document.Add(new Paragraph($"• {component}"));
        }

        // Technologies
        document.Add(new Paragraph("Technologies Used")
            .SetFontSize(14)
            .SetBold());
        foreach (var tech in report.ArchitectureSummary.Technologies)
        {
            document.Add(new Paragraph($"• {tech}"));
        }

        // Security Issues
        AddIssuesToPdf(document, "Security Issues", report.SecurityIssues);

        // Scalability Issues
        AddIssuesToPdf(document, "Scalability Issues", report.ScalabilityIssues);

        // Technical Debt
        AddIssuesToPdf(document, "Technical Debt", report.TechnicalDebtIssues);

        // Modernization Recommendations
        AddRecommendationsToPdf(document, report.ModernizationRecommendations);
    }

    private void AppendIssueHtml(System.Text.StringBuilder html, AnalysisIssue issue)
    {
        html.AppendLine($"<div class='issue severity-{issue.Severity.ToString().ToLower()}'>");
        html.AppendLine($"<h3>{issue.Title}</h3>");
        html.AppendLine($"<p><strong>Severity:</strong> {issue.Severity}</p>");
        html.AppendLine($"<p><strong>Description:</strong> {issue.Description}</p>");
        html.AppendLine($"<p><strong>Location:</strong> {issue.Location}</p>");
        html.AppendLine($"<p><strong>Recommendation:</strong> {issue.Recommendation}</p>");
        html.AppendLine($"<p><strong>Estimated Effort:</strong> {issue.EstimatedEffort}</p>");
        html.AppendLine("</div>");
    }

    private void AppendRecommendationHtml(System.Text.StringBuilder html, ModernizationRecommendation recommendation)
    {
        html.AppendLine($"<div class='issue severity-{recommendation.Severity.ToString().ToLower()}'>");
        html.AppendLine($"<h3>{recommendation.Title}</h3>");
        html.AppendLine($"<p><strong>Category:</strong> {recommendation.Category}</p>");
        html.AppendLine($"<p><strong>Description:</strong> {recommendation.Description}</p>");
        html.AppendLine($"<p><strong>Benefits:</strong> {recommendation.BenefitDescription}</p>");
        html.AppendLine("<p><strong>Prerequisites:</strong></p>");
        html.AppendLine("<ul>");
        foreach (var prerequisite in recommendation.Prerequisites)
        {
            html.AppendLine($"<li>{prerequisite}</li>");
        }
        html.AppendLine("</ul>");
        html.AppendLine("</div>");
    }

    private void AddIssuesToPdf(Document document, string title, IEnumerable<AnalysisIssue> issues)
    {
        document.Add(new Paragraph(title)
            .SetFontSize(16)
            .SetBold());

        foreach (var issue in issues.OrderByDescending(i => i.Severity))
        {
            document.Add(new Paragraph(issue.Title)
                .SetFontSize(14)
                .SetBold());
            document.Add(new Paragraph($"Severity: {issue.Severity}"));
            document.Add(new Paragraph($"Description: {issue.Description}"));
            document.Add(new Paragraph($"Location: {issue.Location}"));
            document.Add(new Paragraph($"Recommendation: {issue.Recommendation}"));
            document.Add(new Paragraph($"Estimated Effort: {issue.EstimatedEffort}"));
            document.Add(new Paragraph("")); // Spacing
        }
    }

    private void AddRecommendationsToPdf(Document document, IEnumerable<ModernizationRecommendation> recommendations)
    {
        document.Add(new Paragraph("Modernization Recommendations")
            .SetFontSize(16)
            .SetBold());

        foreach (var recommendation in recommendations.OrderByDescending(r => r.Severity))
        {
            document.Add(new Paragraph(recommendation.Title)
                .SetFontSize(14)
                .SetBold());
            document.Add(new Paragraph($"Category: {recommendation.Category}"));
            document.Add(new Paragraph($"Description: {recommendation.Description}"));
            document.Add(new Paragraph($"Benefits: {recommendation.BenefitDescription}"));
            
            document.Add(new Paragraph("Prerequisites:")
                .SetBold());
            foreach (var prerequisite in recommendation.Prerequisites)
            {
                document.Add(new Paragraph($"• {prerequisite}"));
            }
            document.Add(new Paragraph("")); // Spacing
        }
    }
}
