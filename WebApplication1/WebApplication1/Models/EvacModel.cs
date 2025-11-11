using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebApplication1.Models
{
    [Table("Tbl_Evacuees")]
    
    public class EvacModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FamilyId { get; set; }

        public string? HeadOfFamily { get; set; }
        public int? Seniors { get; set; }
        public int? PWD { get; set; }
        public int? Children { get; set; }
        public int? TotalMembers { get; set; }
        public string? EvacuationCenterAssigned { get; set; }

        // ✅ these are the 3 you recently added — make them nullable too
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public string? DisasterType { get; set; }

        public int? MappingId { get; set; } // FK
    }
}
