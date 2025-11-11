using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class EvacueeController : Controller
    {
        private readonly EvacDbSet _context;

        public EvacueeController(EvacDbSet context)
        {
            _context = context;
        }

        public async Task<IActionResult> Evacuess()
        {
            // Fetch data from the 'tbl_Evacuess' table
            var evacuees = await _context.Evacuees.ToListAsync();

            // Pass the data to the view
            return View(evacuees);
        }
    }
}
