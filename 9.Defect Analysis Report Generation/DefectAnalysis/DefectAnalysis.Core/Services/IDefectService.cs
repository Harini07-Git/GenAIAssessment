using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefectAnalysis.Core.Models;

namespace DefectAnalysis.Core.Services
{
    public interface IDefectService
    {
        Task<IEnumerable<Defect>> GetDefectsAsync(DateTime startDate, DateTime endDate);
        Task<IDictionary<string, int>> AnalyzeRootCauseDistributionAsync(IEnumerable<Defect> defects);
        Task<IDictionary<string, List<Defect>>> AnalyzeRecurringIssuesAsync(IEnumerable<Defect> defects);
        Task<IDictionary<string, TimeSpan>> CalculateAverageResolutionTimePerSeverityAsync(IEnumerable<Defect> defects);
        Task<IDictionary<string, int>> AnalyzeSeverityDistributionAsync(IEnumerable<Defect> defects);
        Task<List<string>> GenerateRecommendationsAsync(IEnumerable<Defect> defects);
    }
}
