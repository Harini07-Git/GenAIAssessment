namespace ArchitectureAnalyzer.Core;

public interface IArchitectureAnalyzer
{
    Task<AnalysisReport> AnalyzeAsync(string solutionPath);
}

public interface ICodeAnalyzer
{
    Task<ArchitectureSummary> AnalyzeArchitectureAsync(string solutionPath);
}

public interface ISecurityAnalyzer
{
    Task<List<SecurityIssue>> AnalyzeSecurityAsync(string solutionPath);
}

public interface IScalabilityAnalyzer
{
    Task<List<ScalabilityIssue>> AnalyzeScalabilityAsync(string solutionPath);
}

public interface ITechnicalDebtAnalyzer
{
    Task<List<TechnicalDebtIssue>> AnalyzeTechnicalDebtAsync(string solutionPath);
}

public interface IModernizationAnalyzer
{
    Task<List<ModernizationRecommendation>> AnalyzeModernizationOpportunitiesAsync(string solutionPath);
}

public interface IReportGenerator
{
    Task GenerateReportAsync(AnalysisReport report, string outputPath);
}
