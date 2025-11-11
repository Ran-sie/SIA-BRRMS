using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    
    public class EvacuationAreaViewModel
    {
        [Key]
        
        public int MappingId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int? Capacity { get; set; }
        public string? Facilities { get; set; }

    }
}
