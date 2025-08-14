using System;

namespace DefectAnalysis.Core.Models
{
    public class Defect
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Severity { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public string Assignee { get; set; }
        public string RootCause { get; set; }
        public string Component { get; set; }
    }
}
