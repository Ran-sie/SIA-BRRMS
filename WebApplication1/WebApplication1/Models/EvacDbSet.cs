using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class EvacDbSet : DbContext
    {
        public EvacDbSet(DbContextOptions<EvacDbSet> options) : base(options)
        {

        }


        public DbSet<EvacuationAreaViewModel> EvacuationAreas { get; set; }
        public DbSet<MissingView1> MissingReports { get; set; }
        public DbSet<EvacModel> Evacuees { get; set; }
    }
}
