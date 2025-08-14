namespace ArchitectureAnalyzer.Core;

public class AnalysisReport
{
    public string SolutionPath { get; set; } = string.Empty;
    public DateTime AnalysisTimestamp { get; set; }
    public string Version { get; set; } = "1.0.0";
    
    public ArchitectureSummary ArchitectureSummary { get; set; } = new();
    public List<SecurityIssue> SecurityIssues { get; set; } = new();
    public List<ScalabilityIssue> ScalabilityIssues { get; set; } = new();
    public List<TechnicalDebtIssue> TechnicalDebtIssues { get; set; } = new();
    public List<ModernizationRecommendation> ModernizationRecommendations { get; set; } = new();
}

public class ArchitectureSummary
{
    public string CurrentArchitecturePattern { get; set; } = string.Empty;
    public List<string> MainComponents { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
}

public enum Severity
{
    Low,
    Medium,
    High
}

public class AnalysisIssue
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Severity Severity { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string EstimatedEffort { get; set; } = string.Empty;
}

public class SecurityIssue : AnalysisIssue
{
    public string VulnerabilityType { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}

public class ScalabilityIssue : AnalysisIssue
{
    public string BottleneckType { get; set; } = string.Empty;
    public string PerformanceImpact { get; set; } = string.Empty;
}

public class TechnicalDebtIssue : AnalysisIssue
{
    public string DebtCategory { get; set; } = string.Empty;
    public string MaintenanceImpact { get; set; } = string.Empty;
}

public class ModernizationRecommendation : AnalysisIssue
{
    public string Category { get; set; } = string.Empty;
    public string BenefitDescription { get; set; } = string.Empty;
    public List<string> Prerequisites { get; set; } = new();
}
