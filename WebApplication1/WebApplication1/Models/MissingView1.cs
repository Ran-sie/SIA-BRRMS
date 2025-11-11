using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;


namespace WebApplication1.Models
{
    public class MissingView1
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }
        public int? Age { get; set; }
        public DateTime DateMissing { get; set; }
        public string? Status { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Gender { get; set; }
        public string? LastSeenLocation { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }

        [NotMapped]
        public IFormFile? PhotoFile { get; set; }
    }
}
