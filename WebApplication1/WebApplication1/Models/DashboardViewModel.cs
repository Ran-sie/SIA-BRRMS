namespace WebApplication1.Models
{
    public class DashboardViewModel
    {
        public int? TotalEvacuationCenters { get; set; }
        public int? TotalEvacuees { get; set; }
        public int? TotalMissing { get; set; }
        public int? TotalUpcomingDrills { get; set; }

        // For charts
        public int MissingCount { get; set; }
        public int FoundCount { get; set; }

        // For bar chart
        public Dictionary<string, int> FamiliesPerZone { get; set; }

        // Optional for pie chart calculation
        public int EvacuatedCount { get; set; }
        public int SafeCount { get; set; }
    }
}
